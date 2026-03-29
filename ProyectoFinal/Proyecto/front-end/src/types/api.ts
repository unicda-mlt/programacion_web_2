export interface PaginatedResponse<T> {
  data: T[];
  page: number;
  pageSize: number;
  total: number;
}

export interface ApiError {
  title: string;
  detail?: string;
  status?: number;
  errors?: Record<string, string[]>;
}
