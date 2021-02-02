using System;
using System.Collections.Generic;
using System.Text;

namespace Game.Entities
{
    public class TableCreationOrDeletionMessage
    {
        public Entities.Table Table { get; set; }
        public TableAction Action { get; set; }
    }
    public enum TableAction
    {
        Added,
        Removed
    }
}
