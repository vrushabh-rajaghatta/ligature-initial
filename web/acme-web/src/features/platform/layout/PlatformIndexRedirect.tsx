import { Navigate } from "react-router-dom";

/**
 * Users is the only page in this section. The redirect was role-aware while a
 * platform administrator landed on Tenants instead; ADR-066 removed both.
 */
export function PlatformIndexRedirect() {
  return <Navigate to="users" replace />;
}
