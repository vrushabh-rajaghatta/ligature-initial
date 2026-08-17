import { createBrowserRouter, Navigate } from "react-router-dom";
import { HomePage } from "./pages/HomePage";
import { AppLayout } from "@/shared/layout/AppLayout";
import { PlatformLayout } from "@/features/platform/layout/PlatformLayout";
import { PlatformIndexRedirect } from "@/features/platform/layout/PlatformIndexRedirect";
import { UsersPage } from "@/features/platform/users/pages/UsersPage";
import { UserDetailsPage } from "@/features/platform/users/pages/UserDetailsPage";
import { LoginPage } from "@/features/auth/pages/LoginPage";
import { AcceptInvitationPage } from "@/features/auth/pages/AcceptInvitationPage";
import { ForgotPasswordPage } from "@/features/auth/pages/ForgotPasswordPage";
import { ResetPasswordPage } from "@/features/auth/pages/ResetPasswordPage";
import { SettingsLayout } from "@/features/settings/layout/SettingsLayout";
import { SecurityPage } from "@/features/settings/pages/SecurityPage";
import { SessionsPage } from "@/features/settings/pages/SessionsPage";
import { RequireAuth } from "@/features/auth/components/RequireAuth";
import { DocumentsListPage } from "@/features/documents/pages/DocumentsListPage";
import { DocumentDetailPage } from "@/features/documents/pages/DocumentDetailPage";

export const router = createBrowserRouter([
  {
    // Outside the shell: there is no navigation to show someone who has not
    // signed in, and the header links to pages they cannot load.
    path: "/login",
    element: <LoginPage />,
  },
  {
    // Outside RequireAuth: whoever follows this link has no session, and
    // obtaining the ability to have one is the point.
    path: "/accept-invitation",
    element: <AcceptInvitationPage />,
  },
  {
    // Also outside RequireAuth, and for a stronger reason: someone who has
    // forgotten their password cannot sign in to ask for a new one.
    path: "/forgot-password",
    element: <ForgotPasswordPage />,
  },
  {
    path: "/reset-password",
    element: <ResetPasswordPage />,
  },
  {
    path: "/",
    element: <RequireAuth />,
    children: [
      {
        element: <AppLayout />,
        children: [
          {
            index: true,
            element: <HomePage />,
          },
          {
            path: "documents",
            children: [
              {
                index: true,
                element: <DocumentsListPage />,
              },
              {
                path: ":documentId",
                element: <DocumentDetailPage />,
              },
            ],
          },
          {
            path: "settings",
            element: <SettingsLayout />,
            children: [
              {
                index: true,
                element: <Navigate to="security" replace />,
              },
              {
                path: "security",
                element: <SecurityPage />,
              },
              {
                path: "sessions",
                element: <SessionsPage />,
              },
            ],
          },
          {
            path: "platform",
            element: <PlatformLayout />,
            children: [
              {
                index: true,
                element: <PlatformIndexRedirect />,
              },
              {
                path: "users",
                element: <UsersPage />,
              },
              {
                path: "users/:userId",
                element: <UserDetailsPage />,
              },
            ],
          },
        ],
      },
    ],
  },
]);
