import { create } from 'zustand';
import type { Session, OutputChunk, WorkspaceFile } from '../types';

interface AppState {
  // 会话相关
  sessions: Session[];
  currentSession: Session | null;

  // 输出相关
  outputs: OutputChunk[];

  // 工作区相关
  workspaceFiles: WorkspaceFile[];

  // UI状态
  isConnected: boolean;
  isLoading: boolean;
  currentView: 'chat' | 'files' | 'preview';

  // Actions
  setSessions: (sessions: Session[]) => void;
  setCurrentSession: (session: Session | null) => void;
  addOutput: (output: OutputChunk) => void;
  clearOutputs: () => void;
  setWorkspaceFiles: (files: WorkspaceFile[]) => void;
  setIsConnected: (connected: boolean) => void;
  setIsLoading: (loading: boolean) => void;
  setCurrentView: (view: 'chat' | 'files' | 'preview') => void;
}

export const useAppStore = create<AppState>((set) => ({
  // 初始状态
  sessions: [],
  currentSession: null,
  outputs: [],
  workspaceFiles: [],
  isConnected: false,
  isLoading: false,
  currentView: 'chat',

  // Actions
  setSessions: (sessions) => set({ sessions }),

  setCurrentSession: (session) => set({ currentSession: session, outputs: [] }),

  addOutput: (output) => set((state) => ({
    outputs: [...state.outputs, output],
  })),

  clearOutputs: () => set({ outputs: [] }),

  setWorkspaceFiles: (files) => set({ workspaceFiles: files }),

  setIsConnected: (connected) => set({ isConnected: connected }),

  setIsLoading: (loading) => set({ isLoading: loading }),

  setCurrentView: (view) => set({ currentView: view }),
}));
