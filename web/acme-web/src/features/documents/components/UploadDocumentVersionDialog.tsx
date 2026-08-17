import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";

import { UploadDocumentVersionForm } from "./UploadDocumentVersionForm";

interface UploadDocumentVersionDialogProps {
  documentId: string;
  open: boolean;
  onOpenChange: (open: boolean) => void;
}

export function UploadDocumentVersionDialog({
  documentId,
  open,
  onOpenChange,
}: UploadDocumentVersionDialogProps) {
  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>Upload New Version</DialogTitle>
        </DialogHeader>

        <UploadDocumentVersionForm
          documentId={documentId}
          onSuccess={() => onOpenChange(false)}
        />
      </DialogContent>
    </Dialog>
  );
}
