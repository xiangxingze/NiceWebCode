// API响应类型定义

export interface Session {
  id: string;
  title: string;
  userId: string;
  createdAt: string;
  updatedAt: string;
  status: 'Active' | 'Idle' | 'Archived';
  isPublic: boolean;
  shareToken?: string;
}

export interface CreateSessionRequest {
  title: string;
  userId: string;
}

export interface ExecuteTaskRequest {
  prompt: string;
  cliToolName: string;
}

export interface OutputChunk {
  id: string;
  sessionId: string;
  taskId?: string;
  type: 'Text' | 'Code' | 'Markdown' | 'ToolUse' | 'ToolResult' | 'Error' | 'System' | 'JsonlEvent';
  content: string;
  createdAt: string;
  sequenceNumber: number;
  metadata?: Record<string, any>;
}

export interface WorkspaceFile {
  relativePath: string;
  fileName: string;
  fileSize: number;
  contentType: string;
  updatedAt: string;
  isDirectory: boolean;
  children?: WorkspaceFile[];
}

export interface TaskStatusUpdate {
  taskId: string;
  status: string;
}

export interface TaskCompletedEvent {
  taskId: string;
  success: boolean;
  errorMessage?: string;
}
