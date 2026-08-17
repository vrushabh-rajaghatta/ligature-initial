export interface DocumentVersion {
  id: string;
  versionNumber: number;
  isCurrent: boolean;
  originalFileName: string;
  contentType: string;
  fileSize: number;
  checksum: string;
  uploadedOnUtc: string;
}

export interface DocumentDetail {
  id: string;
  name: string;
  status: string;
  createdOnUtc: string;
  versions: DocumentVersion[];
}
