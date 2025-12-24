import { LoginForm } from "../components/LoginForm";
import { CreateGameForm } from "../components/CreateGameForm";

export function LandingPage() {
  return (
    <div className="grid md:grid-cols-2 gap-8 max-w-6xl mx-auto">
      <div>
        <LoginForm />
      </div>
      <div>
        <CreateGameForm />
      </div>
    </div>
  );
}
