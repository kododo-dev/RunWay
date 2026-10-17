import { useCallback } from 'react';
import { useShell } from '@kododo-dev/way-shell';

/**
 * Navigates by a path from the dashboard's root, e.g. `/jobs/42`, through the shell: in the panel the dashboard sits
 * under #/{wayId}/, on its own page under #/. Router links relative to the current route would resolve against the
 * whole path of the shell's splat route.
 */
export function useWayNavigate() {
  const { navigate, wayId } = useShell();
  return useCallback(
    (path: string, options?: { replace?: boolean }) => navigate(wayId, path.replace(/^\//, ''), options),
    [navigate, wayId],
  );
}
