import { z } from "zod";

export const uploadDocumentVersionSchema = z.object({
  file: z.instanceof(File, { message: "A file is required." }),
});

export type UploadDocumentVersionFormValues = z.infer<
  typeof uploadDocumentVersionSchema
>;
