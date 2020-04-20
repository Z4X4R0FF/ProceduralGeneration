using Unity.Serialization.Json;
using Unity.Serialization.Json.Adapters.Contravariant;
using UnityEditor;

namespace Unity.Build
{
    sealed class BuildStepJsonAdapter : IJsonAdapter<IBuildStep>
    {
        [InitializeOnLoadMethod]
        static void Register() => JsonSerialization.AddGlobalAdapter(new BuildStepJsonAdapter());

        public object Deserialize(SerializedValueView view)
        {
            var json = view.ToString();
            if (string.IsNullOrEmpty(json))
            {
                return null;
            }

            if (GlobalObjectId.TryParse(json, out var id))
            {
                if (GlobalObjectId.GlobalObjectIdentifierToObjectSlow(id) is BuildPipeline pipeline)
                {
                    return pipeline;
                }
            }
            else
            {
                if (TypeConstructionHelper.TryConstructFromAssemblyQualifiedTypeName<IBuildStep>(json, out var step))
                {
                    return step;
                }
            }
            return null;
        }

        public void Serialize(JsonStringBuffer writer, IBuildStep value)
        {
            string json = null;
            if (value != null)
            {
                if (value is BuildPipeline pipeline)
                {
                    json = GlobalObjectId.GetGlobalObjectIdSlow(pipeline).ToString();
                }
                else
                {
                    json = value.GetType().GetQualifedAssemblyTypeName();
                }
            }
            writer.WriteEncodedJsonString(json);
        }
    }
}
