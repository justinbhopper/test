using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace TestConsole.JsonTest;

public class ImageEncodingConverter : PolymorphicConverter<ImageEncoding>
{
    public ImageEncodingConverter()
        : base(new Discriminator()) { }

    private class Discriminator : IJsonPolymorphicDiscriminator
    {
        public Type Discriminate(JObject jObject)
        {
            var imageTypeToken = jObject["ImageType"] ?? jObject["imageType"];
            if (imageTypeToken is null || imageTypeToken.Type != JTokenType.String)
                throw new JsonSerializationException($"Property '{nameof(ImageEncoding.ImageType)}' must be present to deserialize type {nameof(ImageEncoding)}.");

            var imageTypeValue = imageTypeToken.Value<string>();

            if (!Enum.TryParse<ImageType>(imageTypeValue, true, out var imageType))
                throw new JsonSerializationException($"Property '{nameof(ImageEncoding.ImageType)}' contained an invalid value.");

            return imageType switch
            {
                ImageType.Png => typeof(PngEncoding),
                ImageType.Jpeg => typeof(JpegEncoding),
                _ => throw new JsonSerializationException($"Property '{nameof(ImageEncoding.ImageType)}' contained an unsupported value."),
            };
        }
    }
}
