import * as signalR from '@microsoft/signalr';
import type { OutputChunk, TaskStatusUpdate, TaskCompletedEvent } from '../types';

const HUB_URL = import.meta.env.VITE_HUB_URL || 'http://localhost:5000/hubs/output';

export class SignalRService {
  private connection: signalR.HubConnection;
  private isConnected = false;
  private currentSessionId: string | null = null;

  // 事件回调
  private onOutputReceived?: (chunk: OutputChunk) => void;
  private onTaskStatusChanged?: (update: TaskStatusUpdate) => void;
  private onTaskCompleted?: (event: TaskCompletedEvent) => void;
  private onConnectionChanged?: (connected: boolean) => void;

  constructor() {
    this.connection = new signalR.HubConnectionBuilder()
      .withUrl(HUB_URL)
      .withAutomaticReconnect({
        nextRetryDelayInMilliseconds: (retryContext) => {
          // 重连延迟策略: 0s, 2s, 10s, 30s
          if (retryContext.previousRetryCount === 0) return 0;
          if (retryContext.previousRetryCount === 1) return 2000;
          if (retryContext.previousRetryCount === 2) return 10000;
          return 30000;
        },
      })
      .configureLogging(signalR.LogLevel.Information)
      .build();

    this.setupEventHandlers();
  }

  private setupEventHandlers() {
    // 接收输出
    this.connection.on('ReceiveOutput', (chunk: OutputChunk) => {
      console.log('[SignalR] ReceiveOutput:', chunk);
      if (this.onOutputReceived) {
        this.onOutputReceived(chunk);
      }
    });

    // 任务状态变更
    this.connection.on('TaskStatusChanged', (taskId: string, status: string) => {
      console.log('[SignalR] TaskStatusChanged:', taskId, status);
      if (this.onTaskStatusChanged) {
        this.onTaskStatusChanged({ taskId, status });
      }
    });

    // 任务完成
    this.connection.on('TaskCompleted', (taskId: string, success: boolean, errorMessage?: string) => {
      console.log('[SignalR] TaskCompleted:', taskId, success, errorMessage);
      if (this.onTaskCompleted) {
        this.onTaskCompleted({ taskId, success, errorMessage });
      }
    });

    // 连接状态事件
    this.connection.onreconnecting(() => {
      console.log('[SignalR] 正在重连...');
      this.isConnected = false;
      if (this.onConnectionChanged) {
        this.onConnectionChanged(false);
      }
    });

    this.connection.onreconnected(async () => {
      console.log('[SignalR] 重连成功');
      this.isConnected = true;
      if (this.onConnectionChanged) {
        this.onConnectionChanged(true);
      }

      // 重新加入会话组
      if (this.currentSessionId) {
        await this.joinSession(this.currentSessionId);
      }
    });

    this.connection.onclose(() => {
      console.log('[SignalR] 连接已关闭');
      this.isConnected = false;
      if (this.onConnectionChanged) {
        this.onConnectionChanged(false);
      }
    });
  }

  // 启动连接
  async start(): Promise<void> {
    if (this.isConnected) {
      console.log('[SignalR] 已经连接');
      return;
    }

    try {
      await this.connection.start();
      this.isConnected = true;
      console.log('[SignalR] 连接成功');
      if (this.onConnectionChanged) {
        this.onConnectionChanged(true);
      }
    } catch (error) {
      console.error('[SignalR] 连接失败:', error);
      throw error;
    }
  }

  // 停止连接
  async stop(): Promise<void> {
    if (!this.isConnected) return;

    try {
      await this.connection.stop();
      this.isConnected = false;
      this.currentSessionId = null;
      console.log('[SignalR] 连接已断开');
    } catch (error) {
      console.error('[SignalR] 断开连接失败:', error);
    }
  }

  // 加入会话组
  async joinSession(sessionId: string): Promise<void> {
    if (!this.isConnected) {
      await this.start();
    }

    try {
      await this.connection.invoke('JoinSession', sessionId);
      this.currentSessionId = sessionId;
      console.log('[SignalR] 已加入会话:', sessionId);
    } catch (error) {
      console.error('[SignalR] 加入会话失败:', error);
      throw error;
    }
  }

  // 离开会话组
  async leaveSession(sessionId: string): Promise<void> {
    if (!this.isConnected) return;

    try {
      await this.connection.invoke('LeaveSession', sessionId);
      if (this.currentSessionId === sessionId) {
        this.currentSessionId = null;
      }
      console.log('[SignalR] 已离开会话:', sessionId);
    } catch (error) {
      console.error('[SignalR] 离开会话失败:', error);
    }
  }

  // 设置事件回调
  setOnOutputReceived(callback: (chunk: OutputChunk) => void) {
    this.onOutputReceived = callback;
  }

  setOnTaskStatusChanged(callback: (update: TaskStatusUpdate) => void) {
    this.onTaskStatusChanged = callback;
  }

  setOnTaskCompleted(callback: (event: TaskCompletedEvent) => void) {
    this.onTaskCompleted = callback;
  }

  setOnConnectionChanged(callback: (connected: boolean) => void) {
    this.onConnectionChanged = callback;
  }

  // 获取连接状态
  getConnectionState(): boolean {
    return this.isConnected;
  }

  getCurrentSessionId(): string | null {
    return this.currentSessionId;
  }
}

// 导出单例
export const signalRService = new SignalRService();
