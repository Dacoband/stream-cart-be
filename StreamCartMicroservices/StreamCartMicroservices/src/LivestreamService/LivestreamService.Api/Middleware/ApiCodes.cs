namespace LivestreamService.Api.Middleware
{
    public static class ApiCodes
    {
        // ───────────────────────────────
        // ✅ SUCCESS CODES
        // ───────────────────────────────
        public const string SUCCESS = "SUCCESS";
        public const string CREATED = "CREATED";
        public const string VALIDATED = "VALIDATED";

        // ───────────────────────────────
        // ❌ CLIENT ERRORS (4xx)
        // ───────────────────────────────
        public const string BAD_REQUEST = "BAD_REQUEST";
        public const string UNAUTHORIZED = "UNAUTHORIZED";
        public const string UNAUTHENTICATED = "UNAUTHENTICATED";
        public const string FORBIDDEN = "FORBIDDEN";
        public const string NOT_FOUND = "NOT_FOUND";
        public const string CONFLICT = "CONFLICT";
        public const string INVALID_INPUT = "INVALID_INPUT";
        public const string NOT_UNIQUE = "NOT_UNIQUE";
        public const string DUPLICATE = "DUPLICATE";
        public const string EXISTED = "EXISTED";
        public const string VALIDATION_FAILED = "VALIDATION_FAILED";

        // ───────────────────────────────
        // 🔒 AUTH & TOKEN ERRORS
        // ───────────────────────────────
        public const string TOKEN_EXPIRED = "TOKEN_EXPIRED";
        public const string TOKEN_INVALID = "TOKEN_INVALID";

        // ───────────────────────────────
        // 💥 SERVER ERRORS (5xx)
        // ───────────────────────────────
        public const string INTERNAL_SERVER_ERROR = "INTERNAL_SERVER_ERROR";
        public const string EXTERNAL_SERVICE_ERROR = "EXTERNAL_SERVICE_ERROR";
        public const string UNKNOWN_ERROR = "UNKNOWN_ERROR";
    }
}
