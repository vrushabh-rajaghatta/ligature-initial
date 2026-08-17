import { Badge } from "@/components/ui/badge";

// Domain status values mapped to user-facing labels.
const STATUS_LABELS: Record<string, string> = {
  Draft: "Draft",
  Active: "Active",
  Archived: "Archived",
};

const STATUS_VARIANTS: Record<string, "default" | "secondary" | "outline"> = {
  Draft: "secondary",
  Active: "default",
  Archived: "outline",
};

interface DocumentStatusBadgeProps {
  status: string;
}

export function DocumentStatusBadge({ status }: DocumentStatusBadgeProps) {
  return (
    <Badge variant={STATUS_VARIANTS[status] ?? "default"}>
      {STATUS_LABELS[status] ?? status}
    </Badge>
  );
}
