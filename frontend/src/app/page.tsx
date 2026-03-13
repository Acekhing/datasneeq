import { Database } from "lucide-react";
import WizardContainer from "@/components/wizard/WizardContainer";

export default function Home() {
  return (
    <div className="min-h-screen bg-background">
      <header className="border-b bg-card">
        <div className="max-w-6xl mx-auto px-4 py-4 flex items-center gap-3">
          <Database className="w-6 h-6 text-primary" />
          <div>
            <h1 className="text-xl font-bold tracking-tight">DataSneeq</h1>
            <p className="text-sm text-muted-foreground">
              Excel Data Upload & Mapping Portal
            </p>
          </div>
        </div>
      </header>
      <main className="px-4 py-8">
        <WizardContainer />
      </main>
    </div>
  );
}
