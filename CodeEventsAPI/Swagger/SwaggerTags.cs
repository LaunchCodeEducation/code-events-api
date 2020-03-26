namespace CodeEventsAPI.Swagger {
  public struct SwaggerTags {
    public const string PublicTag = "Public";
    public const string RequireOwnerTag = "Protected, Authorization: Owner";
    public const string RequireMemberTag = "Protected, Authorization: Member";
    public const string RequireUserTag = "Protected, Authorization: None";
  }
}
