// Transport interface - allows swapping between real HTTP and mock implementations

export interface ITransport {
  get<T>(url: string, headers?: Record<string, string>): Promise<T>;
  post<TReq, TRes>(
    url: string,
    data: TReq,
    headers?: Record<string, string>
  ): Promise<TRes>;
  put<TReq, TRes>(
    url: string,
    data: TReq,
    headers?: Record<string, string>
  ): Promise<TRes>;
  delete<T>(url: string, headers?: Record<string, string>): Promise<T>;
}
