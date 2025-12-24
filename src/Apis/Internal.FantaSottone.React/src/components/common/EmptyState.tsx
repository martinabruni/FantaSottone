import { Inbox } from "lucide-react";

interface EmptyStateProps {
  title?: string;
  message?: string;
}

export function EmptyState({
  title = "No data",
  message = "Nothing to display yet",
}: EmptyStateProps) {
  return (
    <div className="flex flex-col items-center justify-center p-8 space-y-4">
      <Inbox className="h-12 w-12 text-muted-foreground" />
      <div className="text-center space-y-2">
        <h3 className="font-semibold text-lg">{title}</h3>
        <p className="text-sm text-muted-foreground">{message}</p>
      </div>
    </div>
  );
}
