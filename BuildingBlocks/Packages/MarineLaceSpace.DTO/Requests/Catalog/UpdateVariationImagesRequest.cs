namespace MarineLaceSpace.DTO.Requests.Catalog;

public class UpdateVariationImagesRequest
{
    public List<VariationImageAssociationRequest> Associations { get; set; } = [];
}