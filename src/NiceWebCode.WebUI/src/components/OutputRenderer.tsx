import React, { useEffect } from 'react';
import ReactMarkdown from 'react-markdown';
import Prism from 'prismjs';
import 'prismjs/themes/prism-tomorrow.css';
import 'prismjs/components/prism-typescript';
import 'prismjs/components/prism-javascript';
import 'prismjs/components/prism-jsx';
import 'prismjs/components/prism-tsx';
import 'prismjs/components/prism-python';
import 'prismjs/components/prism-csharp';
import 'prismjs/components/prism-java';
import 'prismjs/components/prism-json';
import type { OutputChunk } from '../types';

interface OutputRendererProps {
  chunk: OutputChunk;
}

export const OutputRenderer: React.FC<OutputRendererProps> = ({ chunk }) => {
  useEffect(() => {
    Prism.highlightAll();
  }, [chunk]);

  const renderContent = () => {
    switch (chunk.type) {
      case 'Code':
        return (
          <div className="output-code">
            <pre className="language-typescript">
              <code>{chunk.content}</code>
            </pre>
          </div>
        );

      case 'Markdown':
        return (
          <div className="output-markdown">
            <ReactMarkdown>{chunk.content}</ReactMarkdown>
          </div>
        );

      case 'Error':
        return (
          <div className="output-error">
            <div className="error-icon">⚠️</div>
            <div className="error-content">{chunk.content}</div>
          </div>
        );

      case 'System':
        return (
          <div className="output-system">
            <div className="system-icon">🔧</div>
            <div className="system-content">{chunk.content}</div>
          </div>
        );

      case 'ToolUse':
        return (
          <div className="output-tool-use">
            <div className="tool-icon">🛠️</div>
            <div className="tool-content">
              <strong>Tool Use:</strong> {chunk.content}
            </div>
          </div>
        );

      case 'ToolResult':
        return (
          <div className="output-tool-result">
            <div className="tool-icon">✅</div>
            <div className="tool-content">
              <strong>Tool Result:</strong> {chunk.content}
            </div>
          </div>
        );

      case 'Text':
      default:
        return (
          <div className="output-text">
            <p>{chunk.content}</p>
          </div>
        );
    }
  };

  return (
    <div className={`output-chunk output-chunk-${chunk.type.toLowerCase()}`}>
      {renderContent()}
      <div className="output-meta">
        <span className="output-time">
          {new Date(chunk.createdAt).toLocaleTimeString()}
        </span>
      </div>
    </div>
  );
};
