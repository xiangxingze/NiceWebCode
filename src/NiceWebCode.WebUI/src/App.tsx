import { useEffect } from 'react';
import { Sidebar } from './components/Sidebar';
import { ChatPanel } from './components/ChatPanel';
import { useAppStore } from './store/useAppStore';
import { signalRService } from './services/signalr';
import './App.css';

function App() {
  const { setIsConnected, addOutput } = useAppStore();

  useEffect(() => {
    // 初始化SignalR连接
    const initSignalR = async () => {
      try {
        await signalRService.start();

        // 设置事件回调
        signalRService.setOnOutputReceived((output) => {
          addOutput(output);
        });

        signalRService.setOnConnectionChanged((connected) => {
          setIsConnected(connected);
        });

        signalRService.setOnTaskCompleted((event) => {
          if (!event.success) {
            console.error('任务失败:', event.errorMessage);
          }
        });
      } catch (error) {
        console.error('SignalR连接失败:', error);
      }
    };

    initSignalR();

    // 清理
    return () => {
      signalRService.stop();
    };
  }, []);

  return (
    <div className="app">
      <Sidebar />
      <main className="main-content">
        <ChatPanel />
      </main>
    </div>
  );
}

export default App;
