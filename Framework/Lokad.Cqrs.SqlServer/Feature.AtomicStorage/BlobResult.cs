using System.Data.Linq;

namespace Lokad.Cqrs.Feature.AtomicStorage
{
	public class BlobResult
	{
		public Binary Blob { get; set; }
	}
}
