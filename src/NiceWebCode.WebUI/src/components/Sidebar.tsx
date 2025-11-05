import React, { useEffect, useState } from 'react';
import { useAppStore } from '../store/useAppStore';
import { sessionApi } from '../services/api';
import { signalRService } from '../services/signalr';

export const Sidebar: React.FC = () => {
  const { sessions, currentSession, setSessions, setCurrentSession, clearOutputs, isConnected } = useAppStore();
  const [newSessionTitle, setNewSessionTitle] = useState('');
  const [showNewSessionForm, setShowNewSessionForm] = useState(false);

  useEffect(() => {
    loadSessions();
  }, []);

  const loadSessions = async () => {
    try {
      const sessions = await sessionApi.getSessions('user123');
      setSessions(sessions);
    } catch (error) {
      console.error('加载会话列表失败:', error);
    }
  };

  const handleCreateSession = async (e: React.FormEvent) => {
    e.preventDefault();

    if (!newSessionTitle.trim()) return;

    try {
      const newSession = await sessionApi.createSession({
        title: newSessionTitle,
        userId: 'user123',
      });

      setSessions([newSession, ...sessions]);
      setNewSessionTitle('');
      setShowNewSessionForm(false);
      handleSelectSession(newSession);
    } catch (error) {
      console.error('创建会话失败:', error);
      alert('创建会话失败');
    }
  };

  const handleSelectSession = async (session: typeof currentSession) => {
    if (!session) return;

    // 离开当前会话
    if (currentSession && currentSession.id !== session.id) {
      await signalRService.leaveSession(currentSession.id);
    }

    // 选择新会话
    setCurrentSession(session);
    clearOutputs();

    // 加入SignalR会话组
    try {
      await signalRService.joinSession(session.id);

      // 加载历史输出
      const outputs = await sessionApi.getOutputs(session.id);
      outputs.forEach((output) => useAppStore.getState().addOutput(output));
    } catch (error) {
      console.error('切换会话失败:', error);
    }
  };

  const handleDeleteSession = async (sessionId: string, e: React.MouseEvent) => {
    e.stopPropagation();

    if (!confirm('确定要删除这个会话吗？')) return;

    try {
      await sessionApi.deleteSession(sessionId);
      setSessions(sessions.filter((s) => s.id !== sessionId));

      if (currentSession?.id === sessionId) {
        setCurrentSession(null);
      }
    } catch (error) {
      console.error('删除会话失败:', error);
    }
  };

  return (
    <div className="sidebar">
      {/* 头部 */}
      <div className="sidebar-header">
        <h1 className="app-title">NiceWebCode</h1>
        <div className="connection-status">
          <span className={`status-indicator ${isConnected ? 'connected' : 'disconnected'}`} />
          {isConnected ? '已连接' : '未连接'}
        </div>
      </div>

      {/* 新建会话按钮 */}
      <div className="sidebar-actions">
        {!showNewSessionForm ? (
          <button
            className="btn btn-primary btn-block"
            onClick={() => setShowNewSessionForm(true)}
          >
            ➕ 新建会话
          </button>
        ) : (
          <form onSubmit={handleCreateSession} className="new-session-form">
            <input
              type="text"
              className="form-input"
              placeholder="会话标题..."
              value={newSessionTitle}
              onChange={(e) => setNewSessionTitle(e.target.value)}
              autoFocus
            />
            <div className="form-actions">
              <button type="submit" className="btn btn-sm btn-primary">
                创建
              </button>
              <button
                type="button"
                className="btn btn-sm btn-secondary"
                onClick={() => {
                  setShowNewSessionForm(false);
                  setNewSessionTitle('');
                }}
              >
                取消
              </button>
            </div>
          </form>
        )}
      </div>

      {/* 会话列表 */}
      <div className="sessions-list">
        {sessions.length === 0 ? (
          <div className="sessions-empty">
            <p>暂无会话</p>
          </div>
        ) : (
          sessions.map((session) => (
            <div
              key={session.id}
              className={`session-item ${currentSession?.id === session.id ? 'active' : ''}`}
              onClick={() => handleSelectSession(session)}
            >
              <div className="session-content">
                <h3 className="session-title">{session.title}</h3>
                <p className="session-date">
                  {new Date(session.createdAt).toLocaleDateString()}
                </p>
              </div>
              <button
                className="session-delete-btn"
                onClick={(e) => handleDeleteSession(session.id, e)}
                title="删除会话"
              >
                🗑️
              </button>
            </div>
          ))
        )}
      </div>
    </div>
  );
};
