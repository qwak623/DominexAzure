using System;
using Havit.Collections;
using ProtoBuf;

namespace Dominex.Contracts;

// todo k cemu toto je?
[ProtoContract]
public class DataFragmentRequest<TFilter>
{
	[ProtoMember(1)]
	public TFilter Filter { get; init; }

	[ProtoMember(2)]
	public int StartIndex { get; init; }

	[ProtoMember(3)]
	public int? Count { get; init; }

	[ProtoMember(4)]
	public SortItem[] Sorting { get; init; } = Array.Empty<SortItem>();
}
