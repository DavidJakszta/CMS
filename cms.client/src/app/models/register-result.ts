import { UserResponse } from './user-response';

export interface RegisterResult {
  success: boolean;
  user?: UserResponse;
  suggestedUserName?: string;
  errors: string[];
}
