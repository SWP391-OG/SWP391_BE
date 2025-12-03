namespace SWP391.WebAPI.Constants
{
    public static class ApiStatusCode
    {
        // Success responses (2xx)
        public const int OK = 200;
        public const int CREATED = 201;
        public const int NO_CONTENT = 204;

        // Client error responses (4xx)
        public const int BAD_REQUEST = 400;
        public const int UNAUTHORIZED = 401;
        public const int FORBIDDEN = 403;
        public const int NOT_FOUND = 404;
        public const int CONFLICT = 409;
        public const int UNPROCESSABLE_ENTITY = 422;

        // Server error responses (5xx)
        public const int INTERNAL_SERVER_ERROR = 500;
    }
}