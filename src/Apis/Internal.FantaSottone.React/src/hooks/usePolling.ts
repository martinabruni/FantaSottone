import { useEffect, useRef, useState } from "react";

export interface UsePollingOptions {
  interval: number; // milliseconds
  enabled?: boolean;
  onError?: (error: Error) => void;
}

export function usePolling<T>(
  asyncFunction: () => Promise<T>,
  options: UsePollingOptions
): {
  data: T | null;
  loading: boolean;
  error: Error | null;
  refetch: () => Promise<void>;
} {
  const [data, setData] = useState<T | null>(null);
  const [loading, setLoading] = useState<boolean>(false);
  const [error, setError] = useState<Error | null>(null);

  const intervalRef = useRef<ReturnType<typeof setInterval> | null>(null);
  const isMountedRef = useRef(true);

  const fetchData = async () => {
    if (!isMountedRef.current) return;

    try {
      setLoading(true);
      const result = await asyncFunction();

      if (isMountedRef.current) {
        setData(result);
        setError(null);
      }
    } catch (err) {
      if (isMountedRef.current) {
        const error = err as Error;
        setError(error);
        options.onError?.(error);
      }
    } finally {
      if (isMountedRef.current) {
        setLoading(false);
      }
    }
  };

  useEffect(() => {
    isMountedRef.current = true;

    if (options.enabled === false) {
      return;
    }

    // Initial fetch
    fetchData();

    // Setup polling
    intervalRef.current = setInterval(() => {
      fetchData();
    }, options.interval);

    return () => {
      isMountedRef.current = false;
      if (intervalRef.current) {
        clearInterval(intervalRef.current);
      }
    };
  }, [options.interval, options.enabled]);

  return {
    data,
    loading,
    error,
    refetch: fetchData,
  };
}
