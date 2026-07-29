import { UserResponse } from './user-response';

export interface LoginResult {
  success: boolean;
  token?: string;
  user?: UserResponse;
  errors: string[];
}
