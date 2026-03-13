import axios from 'axios';

/**
 * Extracts a user-friendly error message from API errors (axios) or generic errors.
 * Prefers the backend's `error` field from the JSON response.
 */
export function getApiErrorMessage(error: unknown): string {
  if (axios.isAxiosError(error) && error.response?.data?.error) {
    return error.response.data.error;
  }
  if (error instanceof Error) return error.message;
  return 'An unexpected error occurred. Please try again.';
}
