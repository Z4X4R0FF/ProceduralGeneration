using Unity.Serialization.Json;
using Unity.Serialization.Json.Adapters.Contravariant;
using UnityEditor;

namespace Unity.Build
{
    sealed class RunStepJsonAdapter : IJsonAdapter<RunStep>
    {
        [InitializeOnLoadMethod]
        static void Register() => JsonSerialization.AddGlobalAdapter(new RunStepJsonAdapter());

        public object Deserialize(SerializedValueView view)
        {
            var json = view.ToString();
            if (string.IsNullOrEmpty(json))
            {
                return null;
            }
            return TypeConstructionHelper.TryConstructFromAssemblyQualifiedTypeName<RunStep>(json, out var step) ? step : null;
        }

        public void Serialize(JsonStringBuffer writer, RunStep value)
        {
            string json = null;
            if (value != null)
            {
                json = value.GetType().GetQualifedAssemblyTypeName();
            }
            writer.WriteEncodedJsonString(json);
        }
    }
}
