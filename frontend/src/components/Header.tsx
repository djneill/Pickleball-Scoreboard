import { useAuth } from "../hooks/useAuth";
import { useNavigate } from "react-router-dom";

export default function Header() {
  const { user, logout } = useAuth();
  const navigate = useNavigate();

  const handleLogout = () => {
    logout();
    navigate("/auth");
  };

  return (
    <header className="bg-gradient-to-r from-green-500 to-blue-500 text-white px-4 py-3 shadow-lg">
      <div className="max-w-7xl mx-auto flex items-center justify-between">
        <div className="flex items-center space-x-3">
          <span className="text-2xl">🏓</span>
          <div>
            <h1 className="text-xl font-bold">Pickleball Scoreboard</h1>
            <p className="text-xs text-green-50">{user?.email}</p>
          </div>
        </div>

        <button
          onClick={handleLogout}
          className="bg-white/20 hover:bg-white/30 px-4 py-2 rounded-lg font-semibold transition-colors backdrop-blur-sm"
        >
          Logout
        </button>
      </div>
    </header>
  );
}
