namespace BE.GraphQL;

public record TodoCreateInput(string Title, bool? IsCompleted);

public record TodoUpdateInput(string? Title, bool? IsCompleted);
