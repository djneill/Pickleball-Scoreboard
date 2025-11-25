// Temporary debug component - delete after fixing environment variables
export default function DebugEnv() {
  return (
    <div style={{
      position: 'fixed',
      bottom: 10,
      right: 10,
      background: 'black',
      color: 'lime',
      padding: '10px',
      fontSize: '12px',
      fontFamily: 'monospace',
      maxWidth: '400px',
      borderRadius: '5px',
      zIndex: 9999
    }}>
      <div><strong>Environment Variables:</strong></div>
      <div>VITE_API_URL: {import.meta.env.VITE_API_URL || '❌ NOT SET'}</div>
      <div>VITE_GOOGLE_CLIENT_ID: {import.meta.env.VITE_GOOGLE_CLIENT_ID || '❌ NOT SET'}</div>
    </div>
  );
}
