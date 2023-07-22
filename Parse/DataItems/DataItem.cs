﻿using Parse.DataItems;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Parse
{
    public class DataItem
    {
        public string Name { get; set; }
        public byte[] Data { get; set; } = new byte[0];

        public byte[] EntireData { get; set; }

        public int SectionSize { get; set; }

        public int NextSectionOffset { get; set; }

        public int DataOffsetInSection { get; set; }

        public int DataOffsetInFile { get { return DataOffsetInSection + OffsetInFile; } }

        public int HeaderEndOffset { get; set; }

        public int Nick { get; set; }

        public List<DataItem> SubSections { get; set; } = new List<DataItem>();

        public override string ToString()
        {
            return $"{Name} [0x{OffsetInFile.ToString("x4")}] (0x{SectionSize.ToString("x4")})";
        }

        public string DataStr
        {
            get
            {
                return string.Join(Environment.NewLine,
                    Encoding.ASCII.GetString(Data.Select(c => (c >= 32 && c <= 127) ? c : (byte)63).ToArray())
                    .Split(new char[] { '?' }, StringSplitOptions.RemoveEmptyEntries).Where(x => x.Count() > 1));
            }
        }

        public int OffsetInFile { get; set; }

        public int OffsetInSection { get; set; }

        public DataItem(string name, byte[] data, int offsetInFile, byte[] entireData)
        {
            Name = name;
            Data = data;
            EntireData = entireData;
            OffsetInFile = offsetInFile;
        }

        public static (int, string) GetBackReferenceString(int backReferenceOffset, byte[] _data)
        {
            ByteWalker backReferenceBW = new ByteWalker(_data);
            backReferenceBW.CurrentIndex = (int)((long)backReferenceOffset - 0x80000000) + 0x40;
            var DataItemNameSize = backReferenceBW.GetInt();
            var DataItemName = backReferenceBW.GetString(DataItemNameSize);

            return (DataItemNameSize, DataItemName);
        }


        /// <summary>
        /// Creates DataItem from current index.
        /// Assumes that the data item is exactly at index
        /// </summary>
        /// <returns>Data Item to return</returns>
        public static DataItem CreateDataItem(int pCurrentIndex, byte[] _data)
        {
            ByteWalker bw = new ByteWalker(_data);
            bw.CurrentIndex = pCurrentIndex;
            var DataItemNameSize = bw.GetInt();
            var DataItemName = "";

            if (DataItem.IsSectionNameBackReference(DataItemNameSize))
            {
                (DataItemNameSize, DataItemName) = DataItem.GetBackReferenceString(DataItemNameSize, _data);
            }
            else
            {
                DataItemName = bw.GetString(DataItemNameSize);
                var tmpData = bw.GetBytes(2);
            }



            if (bw.PeekIsDelimiter())
            {
                var t = DataItemFactory.Create(DataItemName, new byte[0], pCurrentIndex, _data);
                t.HeaderEndOffset = bw.CurrentIndex + 4;
                return t;
            }

            var DataItemSize = bw.GetInt();
            var iNextSectionOffset = bw.CurrentIndex;
            var DataItemData = bw.GetBytes(DataItemSize);

            var toReturn = DataItemFactory.Create(DataItemName, DataItemData, pCurrentIndex, _data);
            toReturn.SectionSize = DataItemSize;
            toReturn.NextSectionOffset = iNextSectionOffset + DataItemSize;

            return toReturn;

        }
        public static bool IsSectionNameBackReference(int pSectionNameSize)
        {
            return ((pSectionNameSize >> 31) & 1) == 1;
        }

        public static DataTable ToDataTable(List<DataItem> items)
        {
            var dataTable = new DataTable();

            // Generate Columns
            items.Select((item, index) => new DataColumn(item.Name)).ToList().ForEach(dataTable.Columns.Add);

            // Generate Rows
            var maxDepth = items.Max(x => x.SubSections.Count);
            for (var i = 0; i < maxDepth; i++)
            {
                var row = dataTable.NewRow();
                foreach (DataColumn column in dataTable.Columns)
                {
                    var columnIndex = dataTable.Columns.IndexOf(column);
                    if (items[columnIndex].SubSections.Count > i)
                    {
                        row[columnIndex] = items[columnIndex].SubSections[i].Name;
                    }
                    else
                    {
                        row[columnIndex] = DBNull.Value;
                    }
                }
                dataTable.Rows.Add(row);
            }

            return dataTable;
        }
    }
}
