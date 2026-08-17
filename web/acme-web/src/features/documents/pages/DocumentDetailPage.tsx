import { useState } from "react";
import { Link, useParams } from "react-router-dom";

import { Button } from "@/components/ui/button";
import { Page } from "@/shared/components/Page";
import { PageHeader } from "@/shared/components/PageHeader";

import { useDocument } from "../hooks/useDocument";
import { useActivateDocument } from "../hooks/useActivateDocument";
import { useArchiveDocument } from "../hooks/useArchiveDocument";
import { DocumentStatusBadge } from "../components/DocumentStatusBadge";
import { UploadDocumentVersionDialog } from "../components/UploadDocumentVersionDialog";
import { formatFileSize } from "../utils/formatFileSize";

function LifecycleActions({
  documentId,
  status,
}: {
  documentId: string;
  status: string;
}) {
  const activate = useActivateDocument(documentId);
  const archive = useArchiveDocument(documentId);

  const isPending = activate.isPending || archive.isPending;

  // A failed Activate/Archive must be visibly its own event — the page below
  // still shows the (now stale-looking but correct) document, so without this
  // the user would see nothing happen at all.
  const error = activate.error ?? archive.error;

  return (
    <div className="space-y-2">
      <div className="flex gap-2">
        {status === "Draft" && (
          <Button onClick={() => activate.mutate()} disabled={isPending}>
            {activate.isPending ? "Activating..." : "Activate"}
          </Button>
        )}

        {status === "Active" && (
          <Button
            variant="destructive"
            onClick={() => archive.mutate()}
            disabled={isPending}
          >
            {archive.isPending ? "Archiving..." : "Archive"}
          </Button>
        )}
      </div>

      {error && (
        <p className="text-sm text-destructive" role="alert">
          {error.message}
        </p>
      )}
    </div>
  );
}

export function DocumentDetailPage() {
  const { documentId } = useParams();

  const { data: document, isLoading, error } = useDocument(documentId!);

  const [versionDialogOpen, setVersionDialogOpen] = useState(false);

  if (isLoading) {
    return (
      <Page>
        <p className="text-muted-foreground">Loading Document...</p>
      </Page>
    );
  }

  if (error || !document) {
    return (
      <Page>
        <p className="text-destructive">
          {error instanceof Error ? error.message : "Unable to load Document."}
        </p>

        <Link to="/documents" className="text-primary hover:underline">
          Back to Documents
        </Link>
      </Page>
    );
  }

  // Newest first: the version just uploaded is the one being looked for.
  const versions = [...document.versions].sort(
    (a, b) => b.versionNumber - a.versionNumber,
  );

  return (
    <Page>
      <nav className="text-sm text-muted-foreground">
        <Link to="/documents" className="hover:underline">
          Documents
        </Link>
        <span className="mx-1">›</span>
        <span className="text-foreground">{document.name}</span>
      </nav>

      <PageHeader
        title={document.name}
        description={`Created ${new Date(document.createdOnUtc).toLocaleString()}`}
        actions={
          <div className="flex items-center gap-2">
            <DocumentStatusBadge status={document.status} />

            <Button
              variant="outline"
              onClick={() => setVersionDialogOpen(true)}
            >
              Upload New Version
            </Button>
          </div>
        }
      />

      <UploadDocumentVersionDialog
        documentId={document.id}
        open={versionDialogOpen}
        onOpenChange={setVersionDialogOpen}
      />

      <LifecycleActions documentId={document.id} status={document.status} />

      <section className="space-y-3">
        <h2 className="text-xs font-semibold uppercase tracking-wide text-muted-foreground">
          Versions
        </h2>

        <div className="overflow-x-auto rounded-lg border">
          <table className="w-full text-sm">
            <thead className="border-b bg-muted/40 text-left text-muted-foreground">
              <tr>
                <th className="px-4 py-2 font-medium">Version</th>
                <th className="px-4 py-2 font-medium">File Name</th>
                <th className="px-4 py-2 font-medium">Content Type</th>
                <th className="px-4 py-2 font-medium">Size</th>
                <th className="px-4 py-2 font-medium">Checksum</th>
                <th className="px-4 py-2 font-medium">Uploaded</th>
              </tr>
            </thead>

            <tbody>
              {versions.map((version) => (
                <tr key={version.id} className="border-b last:border-0">
                  <td className="px-4 py-2 font-medium">
                    v{version.versionNumber}
                    {version.isCurrent && (
                      <span className="ml-2 text-xs text-muted-foreground">
                        (current)
                      </span>
                    )}
                  </td>

                  <td className="px-4 py-2">{version.originalFileName}</td>

                  <td className="px-4 py-2 text-muted-foreground">
                    {version.contentType}
                  </td>

                  <td className="px-4 py-2 text-muted-foreground">
                    {formatFileSize(version.fileSize)}
                  </td>

                  <td
                    className="max-w-40 truncate px-4 py-2 font-mono text-xs text-muted-foreground"
                    title={version.checksum}
                  >
                    {version.checksum}
                  </td>

                  <td className="px-4 py-2 text-muted-foreground">
                    {new Date(version.uploadedOnUtc).toLocaleString()}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </section>
    </Page>
  );
}
