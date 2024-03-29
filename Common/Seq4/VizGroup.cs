using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Diagnostics;
using System.IO;
using FileHelper;

namespace LORUtils4
{
	public class LORVizItemGroup4 : LORMemberBase4, iLORMember4, IComparable<iLORMember4>
	// An "Item" in a Visualization file is basically a group!
	{
		protected LORVisualization4 parentVisualization = null;
		public bool Locked = false;
		public string Comment = "";
		public int[] AssignedObjectsNumbers = null;
		public List<iLORMember4> AssignedObjects = new List<iLORMember4>();

		private static readonly string FIELDLocked = " Locked";
		private static readonly string FIELDComment = " Comment";
		private static readonly string FIELDObjectID = " Object";
		
		// SuperStarStuff
		// Since (for now) I don't have SuperStar, and
		// Since (for now) We are not writing out Visualization files
		//   Ignore and throw away this stuff
		public int SSWU = 0;
		public string SSFF = "";
		public bool SSReverseOrder = false;
		public bool SSForceRowColumn = false;
		public int SSRow = 0;
		public int SSColumn = 0;
		public bool SSUseMyOrder = false;
		public bool SSStar = false;
		public int SSMatrixInd = 0;
		public int SSPropColorTemp = 0;
		public static readonly string FIELD_SSWU = " SSWU";
		public static readonly string FIELD_SSFF = " SSFF";
		public static readonly string FIELD_SSReverseOrder = " SSReverseOrder";
		public static readonly string FIELD_SSForceRowColumn = " SSForceRowColumn";
		public static readonly string FIELD_SSRow = " SSRow";
		public static readonly string FIELD_SSColumn = " SSColumn";
		public static readonly string FIELD_SSUseMyOrder = " SSUseMyOrder";
		public static readonly string FIELD_SSStar = " SSStar";
		public static readonly string FIELD_SSMatrixInd = " SSMatrixInd";
		public static readonly string FIELD_SSPropColorTemp = " SSPropColorTemp";
		// So when I get ready to start writing out Visualization Files, I probably won't yet be supporting
		// SuperStar since it is very expensive!  So I can just write out all these defaults in one go.
		public static readonly string FIELDS_SS_DEFAULTS = " SSWU=\"0\" SSFF=\"\" SSReverseOrder=\"False\" SSForceRowColumn=\"False\" SSRow=\"0\" SSColumn=\"0\" SSUseMyOrder=\"False\" SSStar=\"False\" SSMatrixInd=\"0\" SSPropColorTemp=\"0\"";

		public LORVizItemGroup4()
		{ }

		public LORVizItemGroup4(string lineIn)
		{
			Parse(lineIn);
		}

		public int VizID
		{
			get
			{
				return mySavedIndex;
			}
		}

		public int AltVizID
		{
			get
			{
				return myAltSavedIndex;
			}
			set
			{
				myAltSavedIndex = value;
			}
		}

		public new int UniverseNumber
		{
			get
			{
				int un = lutils.UNDEFINED;
				if (AssignedObjects.Count > 0)
				{
					un = AssignedObjects[0].UniverseNumber;
				}
				return un;
			}
		}
		public new int DMXAddress
		{
			get
			{
				int da = lutils.UNDEFINED;
				if (AssignedObjects.Count > 0)
				{
					da = AssignedObjects[0].DMXAddress;
				}
				return da;
			}
		}

		public new LORMemberType4 MemberType
		{
			get
			{
				return LORMemberType4.VizChannel4;
			}
		}

		public new int CompareTo(iLORMember4 other)
		{
			int result = 1; // By default I win!
			
			if (LORMembership4.sortMode == LORMembership4.SORTbyOutput)
			{
				if (AssignedObjects != null)
				{
					result = AssignedObjects[0].UniverseNumber.CompareTo(other.UniverseNumber);
					if (result == 0)
					{
						result = AssignedObjects[0].DMXAddress.CompareTo(other.DMXAddress);
					}
				}
			}
			else
			{
				result = base.CompareTo(other);
			}
			
			return result;
		}
		
		public new iLORMember4 Clone()
		{
			LORVizItemGroup4 newGrp = (LORVizItemGroup4)Clone();
			newGrp.parentVisualization = parentVisualization;
			newGrp.Locked = Locked;
			newGrp.Comment = newGrp.Comment;
			newGrp.AssignedObjectsNumbers = AssignedObjectsNumbers;
			//newGrp.AssignedObjects = AssignedObjects;
			// Use/Keep Defaults for SuperStarStuff
			return newGrp;
		}

		public new iLORMember4 Clone(string newName)
		{
			LORVizItemGroup4 newGrp = (LORVizItemGroup4)this.Clone();
			newGrp.ChangeName(newName);
			return newGrp;
		}

		//public string LineOut()
		//{
			//TODO Add support for writing Visualization files
		//	return "";
		//}

		public new void Parse(string lineIn)
		{
			mySavedIndex = lutils.getKeyValue(lineIn, LORVisualization4.FIELDvizID);
			myName = lutils.getKeyWord(lineIn, lutils.FIELDname);
			Locked = lutils.getKeyState(lineIn, FIELDLocked);
			Comment = lutils.getKeyWord(lineIn, FIELDComment);
		}

		public void ParseAssignedObjectNumbers(StreamReader reader)
		{
			bool keepGoing = true;
			int aoCount = 0;
			while (keepGoing)
			{
				if (reader.EndOfStream) keepGoing = false;
				if (keepGoing)
				{
					string lineIn = reader.ReadLine();
					int iEnd = lineIn.IndexOf("</Item>");
					if (iEnd > 0) keepGoing = false;
					if (keepGoing)
					{
						int o = lutils.getKeyValue(lineIn, FIELDObjectID);
						aoCount++;
						Array.Resize(ref AssignedObjectsNumbers, aoCount);
						AssignedObjectsNumbers[aoCount - 1] = o;
					} // End second KeepGoing test-- not end of <Item>
				} // End first KeepGoing test-- not EndOfStream
			} // End While KeepGoing
		} // End ParseAssignedObjectNumbers



	} // End Class LORVizItemGroup4
} // End Namespace

