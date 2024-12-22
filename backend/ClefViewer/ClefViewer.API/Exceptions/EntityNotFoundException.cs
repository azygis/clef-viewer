namespace ClefViewer.API.Exceptions;

public sealed class EntityNotFoundException(string entityName, object key) : Exception($"{entityName} with key {key} was not found");