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
		public int[] AssignedObjectsNumbers = null;
		
		// Will Contain just DrawObjects (for now anyway)
		public LORMembership4 Members = null;

		private static readonly string FIELDLocked = " Locked";
		private static readonly string FIELDComment = " Comment";
		private static readonly string FIELDObjectID = " Object";
		private static readonly string FIELDAssignedID = "AssignedObject ID";
		
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

		public LORVizItemGroup4(iLORMember4 parent, string lineIn)
		{
			
			base.SetParent(parent);
			myParent = parent;
			Members = new LORMembership4(this);
			MakeDummies();
			Parse(lineIn);

		}

		private void MakeDummies()
		{
			if (Members.Count == 0)
			{
				// SavedIndices and SaveIDs in Sequences start at 0. Cool! Great! No Prob!
				// But Channels, Groups, and DrawObjects in Visualizations start at 1 (Grrrrr)
				// So add a dummy object at the [0] start of the lists
				LORVisualization4 vp = (LORVisualization4)myParent;


				LORVizDrawObject4 lvdo = new LORVizDrawObject4(myParent, "DUMMY");
				lvdo.SetIndex(0);
				lvdo.SetID(0);



				//LORVizDrawObject4 lvdo = vp.VizDrawObjects[0];
				Members.Add(lvdo);
			}
		}

		public int ItemID
		{ get { return myID; } }
		public void SetItemID(int newItemID)
		{ myID = newItemID; }
		public int AltItemID
		{ get { return myAltID; } set { myAltID = value; } }
		public override int UniverseNumber
		{
			get
			{
				int un = lutils.UNDEFINED;
				if (Members.Count > 1)
				{
					un = Members.Items[1].UniverseNumber;
				}
				return un;
			}
		}
		public override int DMXAddress
		{
			get
			{
				int da = lutils.UNDEFINED;
				if (Members.Count > 1)
				{
					da = Members.Items[1].DMXAddress;
				}
				return da;
			}
		}

		public override LORMemberType4 MemberType
		{
			get
			{
				return LORMemberType4.VizItemGroup;
			}
		}

		public override int CompareTo(iLORMember4 other)
		{
			int result = 1; // By default I win!
			
			if (LORMembership4.sortMode == LORMembership4.SORTbyOutput)
			{
				if (Members != null)
				{
					result = Members.Items[0].UniverseNumber.CompareTo(other.UniverseNumber);
					if (result == 0)
					{
						result = Members.Items[0].DMXAddress.CompareTo(other.DMXAddress);
					}
				}
			}
			else
			{
				result = base.CompareTo(other);
			}
			
			return result;
		}
		
		public override iLORMember4 Clone()
		{
			LORVizItemGroup4 newGrp = (LORVizItemGroup4)Clone();
			newGrp.parentVisualization = parentVisualization;
			newGrp.Locked = Locked;
			newGrp.Comment = newGrp.Comment;
			newGrp.Members = Members;
			//newGrp.AssignedObjects = AssignedObjects;
			// Use/Keep Defaults for SuperStarStuff
			return newGrp;
		}

		public override iLORMember4 Clone(string newName)
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

		public override void Parse(string lineIn)
		{
			myID = lutils.getKeyValue(lineIn, LORVisualization4.FIELDvizID);
			myIndex = myID-1;
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
						int ox = lutils.getKeyValue(lineIn, FIELDAssignedID);
						int oid = lutils.getKeyValue(lineIn, FIELDObjectID);
						if (AssignedObjectsNumbers == null)
						{
							Array.Resize(ref AssignedObjectsNumbers, ox+1);
							AssignedObjectsNumbers[ox] = oid;
						}
						else
						{
							int c = AssignedObjectsNumbers.Length;
							Array.Resize(ref AssignedObjectsNumbers, ox + 1);
							AssignedObjectsNumbers[ox] = oid;
						}


						aoCount++;
					} // End second KeepGoing test-- not end of <Item>
				} // End first KeepGoing test-- not EndOfStream
			} // End While KeepGoing
		} // End ParseAssignedObjectNumbers

		public override int color
		{
			get
			{
				if (Members.Count > 1)
				{
					return Members[1].color;
				}
				else
				{
					return 0;
				}
			}
			set
			{
				int ignore = value;
			}
		}

		public override Color Color
		{
			get { return lutils.Color_LORtoNet(this.color); }
			set { Color ignore = value; }
		}


	} // End Class LORVizItemGroup4
} // End Namespace

