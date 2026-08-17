import { z } from "zod";

export const uploadDocumentSchema = z.object({
  name: z.string().trim().min(1, "Document name is required."),

  file: z.instanceof(File, { message: "A file is required." }),
});

export type UploadDocumentFormValues = z.infer<typeof uploadDocumentSchema>;
