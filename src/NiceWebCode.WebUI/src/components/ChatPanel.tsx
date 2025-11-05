import React, { useState, useRef, useEffect } from 'react';
import { OutputRenderer } from './OutputRenderer';
import { useAppStore } from '../store/useAppStore';
import { sessionApi } from '../services/api';

export const ChatPanel: React.FC = () => {
  const { currentSession, outputs, isLoading, setIsLoading } = useAppStore();
  const [prompt, setPrompt] = useState('');
  const outputsEndRef = useRef<HTMLDivElement>(null);

  // 自动滚动到底部
  useEffect(() => {
    outputsEndRef.current?.scrollIntoView({ behavior: 'smooth' });
  }, [outputs]);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    if (!prompt.trim() || !currentSession) return;

    setIsLoading(true);

    try {
      await sessionApi.executeTask(currentSession.id, {
        prompt: prompt.trim(),
        cliToolName: 'claude-code',
      });

      setPrompt('');
    } catch (error) {
      console.error('执行任务失败:', error);
      alert('执行任务失败，请检查后端服务是否正常运行');
    } finally {
      setIsLoading(false);
    }
  };

  if (!currentSession) {
    return (
      <div className="chat-panel chat-panel-empty">
        <div className="empty-state">
          <h2>👋 欢迎使用 NiceWebCode</h2>
          <p>请在左侧创建或选择一个会话开始使用</p>
        </div>
      </div>
    );
  }

  return (
    <div className="chat-panel">
      {/* 输出区域 */}
      <div className="chat-outputs">
        {outputs.length === 0 ? (
          <div className="outputs-empty">
            <p>💬 输入提示词开始与AI对话...</p>
          </div>
        ) : (
          outputs.map((output) => (
            <OutputRenderer key={output.id} chunk={output} />
          ))
        )}
        <div ref={outputsEndRef} />
      </div>

      {/* 输入区域 */}
      <form className="chat-input-form" onSubmit={handleSubmit}>
        <textarea
          className="chat-input"
          placeholder="输入你的提示词... (例如: 创建一个贪吃蛇游戏)"
          value={prompt}
          onChange={(e) => setPrompt(e.target.value)}
          onKeyDown={(e) => {
            if (e.key === 'Enter' && !e.shiftKey) {
              e.preventDefault();
              handleSubmit(e);
            }
          }}
          disabled={isLoading}
          rows={3}
        />
        <button
          type="submit"
          className="chat-submit-btn"
          disabled={isLoading || !prompt.trim()}
        >
          {isLoading ? '执行中...' : '发送'}
        </button>
      </form>
    </div>
  );
};
