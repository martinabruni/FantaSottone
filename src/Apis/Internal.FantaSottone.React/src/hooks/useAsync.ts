import { useState, useCallback } from "react";

export interface AsyncState<T> {
  data: T | null;
  loading: boolean;
  error: Error | null;
}

export interface UseAsyncReturn<T, TArgs extends unknown[]> {
  data: T | null;
  loading: boolean;
  error: Error | null;
  execute: (...args: TArgs) => Promise<T | undefined>;
  reset: () => void;
}

export function useAsync<T, TArgs extends unknown[] = []>(
  asyncFunction: (...args: TArgs) => Promise<T>
): UseAsyncReturn<T, TArgs> {
  const [state, setState] = useState<AsyncState<T>>({
    data: null,
    loading: false,
    error: null,
  });

  const execute = useCallback(
    async (...args: TArgs): Promise<T | undefined> => {
      setState({ data: null, loading: true, error: null });

      try {
        const data = await asyncFunction(...args);
        setState({ data, loading: false, error: null });
        return data;
      } catch (error) {
        setState({ data: null, loading: false, error: error as Error });
        return undefined;
      }
    },
    [asyncFunction]
  );

  const reset = useCallback(() => {
    setState({ data: null, loading: false, error: null });
  }, []);

  return {
    ...state,
    execute,
    reset,
  };
}
