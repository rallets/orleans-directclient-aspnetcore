﻿using ProtoBuf;
using System;
using System.Collections.Generic;

namespace GrainInterfaces.Products
{
    [ProtoContract]
    [Serializable]
    public class ProductsState
    {
        [ProtoMember(1)]
        public List<Guid> Products = new List<Guid>();
    }
}
