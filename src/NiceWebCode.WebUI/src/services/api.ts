import axios from 'axios';
import type { Session, CreateSessionRequest, ExecuteTaskRequest, OutputChunk, WorkspaceFile } from '../types';

const API_BASE_URL = import.meta.env.VITE_API_URL || 'http://localhost:5000/api';

const apiClient = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Session API
export const sessionApi = {
  // 获取所有会话
  getSessions: async (userId?: string): Promise<Session[]> => {
    const params = userId ? { userId } : {};
    const response = await apiClient.get<Session[]>('/sessions', { params });
    return response.data;
  },

  // 获取单个会话
  getSession: async (sessionId: string): Promise<Session> => {
    const response = await apiClient.get<Session>(`/sessions/${sessionId}`);
    return response.data;
  },

  // 创建会话
  createSession: async (request: CreateSessionRequest): Promise<Session> => {
    const response = await apiClient.post<Session>('/sessions', request);
    return response.data;
  },

  // 执行任务
  executeTask: async (sessionId: string, request: ExecuteTaskRequest): Promise<{ taskId: string }> => {
    const response = await apiClient.post<{ taskId: string }>(`/sessions/${sessionId}/execute`, request);
    return response.data;
  },

  // 获取会话输出
  getOutputs: async (sessionId: string): Promise<OutputChunk[]> => {
    const response = await apiClient.get<OutputChunk[]>(`/sessions/${sessionId}/outputs`);
    return response.data;
  },

  // 删除会话
  deleteSession: async (sessionId: string): Promise<void> => {
    await apiClient.delete(`/sessions/${sessionId}`);
  },
};

// Workspace API
export const workspaceApi = {
  // 获取工作区文件列表
  getFiles: async (sessionId: string): Promise<WorkspaceFile[]> => {
    const response = await apiClient.get<WorkspaceFile[]>(`/workspace/${sessionId}/files`);
    return response.data;
  },

  // 读取文件内容
  getFileContent: async (sessionId: string, path: string): Promise<string> => {
    const response = await apiClient.get<{ content: string }>(`/workspace/${sessionId}/files/content`, {
      params: { path },
    });
    return response.data.content;
  },
};
