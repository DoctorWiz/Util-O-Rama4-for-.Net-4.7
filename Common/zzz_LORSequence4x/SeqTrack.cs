using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Diagnostics;
using FileHelper;

namespace LORUtils4
{


	public class LORTrack4 : LORMemberBase4, iLORMember4, IComparable<iLORMember4>
	{
		// Tracks are the ultimate top-level groups.  Level 2 and up are handled by 'ChannelGroups'
		// Channel groups are optional, a sequence many not have any groups, but it will always have at least one track
		// Tracks do not have savedIndexes.  They are just numbered instead.
		// Tracks can contain regular Channels, RGB Channels, and Channel Groups, but not other tracks
		// (ie: Tracks cannot be nested like Channel Groups (which can be nested many levels deep))
		// All Channel Groups, regular Channels, and RGB Channels must directly or indirectly belong to one or more tracks.
		// Channels, RGB Channels, and channel groups will not be displayed and will not be accessible unless added to one or
		// more tracks, directly or subdirectly (a subitem of a group in a track).
		// A LORTrack4 may not contain more than one copy of the same item-- directly.  Within a subgroup is OK.
		// (See related notes in the LORChannelGroup4 class)

		public LORMembership4 Members; // = new LORMembership4(null);
		public List<LORLoopLevel4> loopLevels = new List<LORLoopLevel4>();
		public LORTimings4 timingGrid = null;

		//! CONSTRUCTORS
		public LORTrack4(string theName, int theTrackNo)
		{
			Members = new LORMembership4(this);
			myName = theName;
			myIndex = theTrackNo - 1;  // Tracks are numbered starting with 1, Indexes start with 0
																 //Members.Parent = ID.Parent;
		}

		public LORTrack4(string lineIn)
		{
			Members = new LORMembership4(this);
			string seek = lutils.STFLD + LORSequence4.TABLEtrack + lutils.FIELDtotalCentiseconds;
			//int pos = lineIn.IndexOf(seek);
			int pos = lutils.ContainsKey(lineIn, seek);
			if (pos > 0)
			{
				Parse(lineIn);
			}
			else
			{
				myName = lineIn;
			}
		}



		//! PROPERTIES, METHODS, ETC.
		public new int Centiseconds
		{
			get
			{
				//return myCentiseconds;
				int cs = 0;
				for (int idx = 0; idx < Members.Count; idx++)
				{
					iLORMember4 mbr = Members[idx];
					if (Members.Items[idx].Centiseconds > cs)
					{
						cs = Members.Items[idx].Centiseconds;
						myCentiseconds = cs;
					}
				}
				return cs;
			}
			set
			{
				if (value != myCentiseconds)
				{
					if (value > lutils.MAXCentiseconds)
					{
						string m = "WARNING!! Setting Centiseconds to more than 60 minutes!  Are you sure?";
						Fyle.WriteLogEntry(m, "Warning");
						if (Fyle.isWiz)
						{
							System.Diagnostics.Debugger.Break();
						}
					}
					else
					{
						if (value < lutils.MINCentiseconds)
						{
							string m = "WARNING!! Setting Centiseconds to less than 1 second!  Are you sure?";
							Fyle.WriteLogEntry(m, "Warning");
							if (Fyle.isWiz)
							{
								System.Diagnostics.Debugger.Break();
							}
						}
						else
						{
							myCentiseconds = value;
							for (int idx = 0; idx < Members.Count; idx++)
							{
								iLORMember4 mbr = Members[idx];
								if (Members.Items[idx].Centiseconds > value)
								{
									Members.Items[idx].Centiseconds = value;
								}
							}

							if (myParent != null)
							{
								if (myParent.Centiseconds < value)
								{
									myParent.Centiseconds = value;
								}
							}
							if (myParent != null) myParent.MakeDirty(true);
						}
					}
				}
			}
		}


		public new LORMemberType4 MemberType
		{
			get
			{
				return LORMemberType4.Track;
			}
		}


		public new string LineOut()
		{
			return LineOut(false);
		}


		public new void Parse(string lineIn)
		{
			this.Members = new LORMembership4(this);
			myName = lutils.HumanizeName(lutils.getKeyWord(lineIn, lutils.FIELDname));
			//mySavedIndex = lutils.getKeyValue(lineIn, lutils.FIELDsavedIndex);
			myCentiseconds = lutils.getKeyValue(lineIn, lutils.FIELDtotalCentiseconds);
			int tempGridSaveID = lutils.getKeyValue(lineIn, LORSequence4.TABLEtimingGrid);
			if (myParent == null)
			{
				// If the parent has not been assigned yet, there is no way to get ahold of the grid
				// So temporarily set the AltSavedIndex to this LORTrack4's LORTimings4's SaveID
				//myAltSavedIndex = tempGridSaveID;
			}
			else
			{
				if (tempGridSaveID < 0)
				{
					// For Channel Configs, there will be no timing grid
					timingGrid = null;
				}
				else
				{
					// Assign the LORTimings4 based on the SaveID
					//LORMember4 member = myParent.Members.bySaveID[tempGridSaveID];
					LORTimings4 tg = null;
					LORSequence4 mySeq = (LORSequence4)myParent;
					for (int i = 0; i < mySeq.TimingGrids.Count; i++)
					{
						if (mySeq.TimingGrids[i].SaveID == tempGridSaveID)
						{
							tg = mySeq.TimingGrids[i];
							i = mySeq.TimingGrids.Count; // force exit of loop
						}
					}
					if (tg == null)
					{
						string msg = "ERROR: Timing Grid with SaveID of " + tempGridSaveID.ToString() + " not found!";
						System.Diagnostics.Debugger.Break();
					}
					timingGrid = tg;
				}
			}
			if (myParent != null) myParent.MakeDirty(true);
		}


		public new iLORMember4 Clone()
		{
			LORTrack4 tr = (LORTrack4)Clone();
			tr.timingGrid = (LORTimings4)timingGrid.Clone();
			return tr;
		}

		public new iLORMember4 Clone(string newName)
		{
			LORTrack4 tr = (LORTrack4)this.Clone();
			ChangeName(newName);
			return tr;
		}

		public int TrackNumber
		{
			get
			{
				// LORTrack4 numbers are one based, the index is zero based, so just add 1 to the index for the track number
				return myIndex + 1;
			}
			// Read-Only!
			//set
			//{
			//	myIndex = value;
			//	if (myParent != null) myParent.MakeDirty(true);
			//}
		}


		public string LineOut(bool selectedOnly)
		{
			StringBuilder ret = new StringBuilder();
			// Write info about track
			ret.Append(lutils.LEVEL2);
			ret.Append(lutils.STFLD);
			ret.Append(LORSequence4.TABLEtrack);
			//! LOR writes it with the Name last
			// In theory, it shouldn't matter
			//if (Name.Length > 1)
			//{
			//	ret += lutils.SPC + FIELDname + lutils.FIELDEQ + Name + lutils.ENDQT;
			//}
			ret.Append(lutils.FIELDtotalCentiseconds);
			ret.Append(lutils.FIELDEQ);
			ret.Append(myCentiseconds.ToString());
			ret.Append(lutils.ENDQT);

			int altID = timingGrid.AltSavedIndex;
			ret.Append(lutils.SPC);
			ret.Append(LORSequence4.TABLEtimingGrid);
			ret.Append(lutils.FIELDEQ);
			ret.Append(altID.ToString());
			ret.Append(lutils.ENDQT);
			// LOR writes it with the Name last
			if (myName.Length > 1)
			{
				ret.Append(lutils.FIELDname);
				ret.Append(lutils.FIELDEQ);
				ret.Append(lutils.XMLifyName(myName));
				ret.Append(lutils.ENDQT);
			}
			ret.Append(lutils.FINFLD);

			ret.Append(lutils.CRLF);
			ret.Append(lutils.LEVEL3);
			ret.Append(lutils.STFLD);
			ret.Append(lutils.TABLEchannel);
			ret.Append(lutils.PLURAL);
			ret.Append(lutils.FINFLD);

			// LORLoop4 thru all items in this track
			foreach (iLORMember4 subID in Members.Items)
			{
				bool sel = subID.Selected;
				if (!selectedOnly || sel)
				{
					// Write out the links to the items
					//masterSI = updatedTracks[trackIndex].newSavedIndexes[iti];

					//if (subID.Name.IndexOf("lyphonic") > 0) System.Diagnostics.Debugger.Break();

					int siAlt = subID.AltSavedIndex;
					if (siAlt > lutils.UNDEFINED)
					{
						ret.Append(lutils.CRLF);
						ret.Append(lutils.LEVEL4);
						ret.Append(lutils.STFLD);
						ret.Append(lutils.TABLEchannel);

						ret.Append(lutils.FIELDsavedIndex);
						ret.Append(lutils.FIELDEQ);
						ret.Append(siAlt.ToString());
						ret.Append(lutils.ENDQT);
						ret.Append(lutils.ENDFLD);
					}
				}
			}

			// Close the list of items
			ret.Append(lutils.CRLF);
			ret.Append(lutils.LEVEL3);
			ret.Append(lutils.FINTBL);
			ret.Append(lutils.TABLEchannel);
			ret.Append(lutils.PLURAL);
			ret.Append(lutils.FINFLD);

			// Write out any LoopLevels in this track
			//writeLoopLevels(trackIndex);
			if (loopLevels.Count > 0)
			{
				ret.Append(lutils.CRLF);
				ret.Append(lutils.LEVEL3);
				ret.Append(lutils.STFLD);
				ret.Append(LORSequence4.TABLEloopLevels);
				ret.Append(lutils.FINFLD);
				foreach (LORLoopLevel4 ll in loopLevels)
				{
					ret.Append(lutils.CRLF);
					ret.Append(ll.LineOut());
				}
				ret.Append(lutils.CRLF);
				ret.Append(lutils.LEVEL3);
				ret.Append(lutils.FINTBL);
				ret.Append(LORSequence4.TABLEloopLevels);
				ret.Append(lutils.FINFLD);
			}
			else
			{
				ret.Append(lutils.CRLF);
				ret.Append(lutils.LEVEL3);
				ret.Append(lutils.STFLD);
				ret.Append(LORSequence4.TABLEloopLevels);
				ret.Append(lutils.ENDFLD);
			}
			ret.Append(lutils.CRLF);
			ret.Append(lutils.LEVEL2);
			ret.Append(lutils.FINTBL);
			ret.Append(LORSequence4.TABLEtrack);
			ret.Append(lutils.FINFLD);


			return ret.ToString();
		}

		public int AddItem(iLORMember4 newPart)
		{
			int retSI = lutils.UNDEFINED;
			bool alreadyAdded = false;
			for (int i = 0; i < Members.Count; i++)
			{
				if (newPart.SavedIndex == Members.Items[i].SavedIndex)
				{
					//TODO: Using saved index, look up Name of item being added
					string sMsg = newPart.Name + " has already been added to this LORTrack4 '" + myName + "'.";
					//DialogResult rs = MessageBox.Show(sMsg, "LORChannel4 Groups", MessageBoxButtons.OK, MessageBoxIcon.Warning);
					if (System.Diagnostics.Debugger.IsAttached)
						//System.Diagnostics.Debugger.Break();
						//TODO: Make this just a warning, put "add" code below into an else block
						//TODO: Do the same with Tracks
						alreadyAdded = true;
					retSI = newPart.SavedIndex;
					i = Members.Count; // Break out of loop
				}
			}
			if (!alreadyAdded)
			{
				retSI = Members.Add(newPart);
				if (myParent != null) myParent.MakeDirty(true);
			}
			return retSI;
		}

		public int AddItem(int itemSavedIndex)
		{
			int ret = lutils.UNDEFINED;
			if (myParent != null)
			{
				LORSequence4 mySeq = (LORSequence4)myParent;
				iLORMember4 newItem = mySeq.Members.FindBySavedIndex(itemSavedIndex);
				if (newItem != null)
				{
					ret = AddItem(newItem);
					myParent.MakeDirty(true);
				}
				else
				{
					if (Fyle.isWiz)
					{
						// Trying to add a member which does not exist!
						System.Diagnostics.Debugger.Break();
					}
				}
			}
			return ret;
		}

		public LORLoopLevel4 AddLoopLevel(string lineIn)
		{
			LORLoopLevel4 newLL = new LORLoopLevel4(lineIn);
			AddLoopLevel(newLL);
			if (myParent != null) myParent.MakeDirty(true);
			return newLL;
		}

		public int AddLoopLevel(LORLoopLevel4 newLL)
		{
			loopLevels.Add(newLL);
			if (myParent != null) myParent.MakeDirty(true);
			return loopLevels.Count - 1;
		}

		public new int UniverseNumber
		{
			get
			{
				if (Members.Count > 0)
				{
					return Members[0].UniverseNumber;
				}
				else
				{
					return 0;
				}
			}
		}
		public new int DMXAddress
		{
			get
			{
				if (Members.Count > 0)
				{
					return Members[0].DMXAddress;
				}
				else
				{
					return 0;
				}
			}
		}

		//TODO: add RemoveItem procedure
	} // end class track
}
