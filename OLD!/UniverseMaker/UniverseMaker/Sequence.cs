﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Windows;
using System.Windows.Forms;


namespace LORUtils
{

	enum fileType { Unrecognized, Musical, Animated, ChannelConfig };
	enum deviceType { None, LOR, DMX, Digital };
	public enum effectType { None, intensity, shimmer, twinkle, DMX }
	enum timingGridType { None, freeform, GRfixed };
	enum tableType { None, channel, rgbChannel, channelGroupList }


	class channel
	{
		public string name;
		public long color;
		public long centiseconds;
		public deviceType deviceType = deviceType.None;
		public int circuit = -1;
		public int network = -1;
		public int unit = -1;
		public int savedIndex = -1;
		public int firstEffectIndex = -1;
		public int finalEffectIndex = -1;
		public bool written = false;
	}

	class savedIndex
	{
		public tableType objType;
		public int objIndex;
	}

	class rgbChannel
	{
		public string name;
		public long totalCentiseconds;
		public int savedIndex;
		public int redChannelIndex;
		public int grnChannelIndex;
		public int bluChannelIndex;
		public int redSavedIndex;
		public int grnSavedIndex;
		public int bluSavedIndex;
		public bool written = false;
	}

	class channelGroupList
	{
		public int channelIndex;
		public int channelSavedIndex;
		public string name;
		public long totalCentiSeconds;
		public int savedIndex;
		public bool written = false;
	}

	class channelGroupItem
	{
		public int channelGroupListIndex;
		public int channelGroupSavedIndex;
		//public int channelSavedIndex;
	}

	class effect
	{
		public int channelIndex = -1;
		public int savedIndex = -1;
		public effectType type = effectType.None;
		public long startCentisecond = -1;
		public long endCentisecond = 9999999999L;
		public int intensity = -1;
		public int startIntensity = -1;
		public int endIntensity = -1;
	}


	class timingGrid
	{
		public string name = "";
		public int saveID = -1;
		public timingGridType type = timingGridType.None;
		public int spacing = -1;
	}

	class timingGridItem
	{
		public int gridIndex = -1;
		public long centisecond = -1;
	}

	class track
	{
		public string name = "";
		public long totalCentiseconds = 0;
		public int timingGridIndex = -1;
		public int timingGridSaveID = -1;
		public int trackItemCount = 0;
	}

	class trackItem
	{
		public int trackIndex = -1;
		public int savedIndex = -1;
	}


	class Sequence
	{

		private const string FILEsequence = "sequence";
		private const string FILEchannelConfig = "channelConfig";
		private const string FIELDmusicFilename = "musicFilename";

		private const string XMLversion = "xml version";
		private const string TABLEchannel = "channel";
		private const string FIELDname = "name";
		private const string FIELDcolor = "color";
		private const string FIELDcentiseconds = "centiseconds";
		private const string FIELDdeviceType = "deviceType";
		private const string FIELDcircuit = "circuit";
		private const string FIELDnetwork = "network";
		private const string FIELDunit = "unit";
		private const string FIELDsavedIndex = "savedIndex";

		private const string TABLErgbChannel = "rgbChannel";
		private const string TABLEchannelGroup = "channelGroup";
		private const string TABLEchannelGroupList = "channelGroupList";
		private const string FIELDchannelGroup = "channelGroup";

		private const string TABLEeffect = "effect";
		private const string FIELDtype = "type";
		private const string FIELDcentisecond = "centisecond";
		private const string FIELDtotalCentiseconds = "totalCentiseconds";
		private const string FIELDstartCentisecond = "startCentisecond";
		private const string FIELDendCentisecond = "endCentisecond";
		private const string FIELDintensity = "intensity";
		private const string FIELDstartIntensity = "startIntensity";
		private const string FIELDendIntensity = "endIntensity";
		private const string TABLEtimingGrid = "timingGrid";
		private const string FIELDsaveID = "saveID";
		private const string TABLEtiming = "timing";
		private const string FIELDspacing = "spacing";
		private const string TABLEtrack = "track";


		private const string SPC = " ";
		private const string LEVEL1 = "\t";
		private const string LEVEL2 = "\t\t";
		private const string LEVEL3 = "\t\t\t";
		private const string LEVEL4 = "\t\t\t\t";
		private const string PLURAL = "s";
		private const string FIELDEQ = "=\"";
		private const string ENDQT = "\"";
		private const string ENDFLD = "/>";
		private const string STFLD = "<";
		private const string FINTBL = "</";
		private const string FINFLD = ">";

		private const string DEVICElor = "LOR";
		private const string DEVICEdmx = "DMX Universe";
		private const string DEVICEdigital = "Digital IO";

		private const string EFFECTintensity = "intensity";
		private const string EFFECTshimmer = "shimmer";
		private const string EFFECTtwinkle = "twinkle";
		private const string EFFECTdmx = "DMX intensity";
		private const string GRIDfreeform = "freeform";
		private const string GRIDfixed = "fixed";

		private StreamWriter writer;
		private string lineOut = ""; // line to be written out, gets modified if necessary
		private int curSavedIndex = 0;
		private string dbgMsg = "";

		public channel[] channels;
		public savedIndex[] savedIndexes;
		public rgbChannel[] rgbChannels;
		public channelGroupList[] channelGroups;
		public channelGroupItem[] channelGroupItems;
		public effect[] effects;
		public timingGrid[] timingGrids;
		public timingGridItem[] timingGridItems;
		public track[] tracks;
		public trackItem[] trackItems;

		public int lineCount = 0;
		public int channelCount = 0;
		public int rgbChannelCount = 0;
		public int groupCount = 0;
		public int groupItemCount = 0;
		public int effectCount = 0;
		public int timingGridCount = 0;
		public int gridItemCount = 0;
		public int trackCount = 0;
		public int trackItemCount = 0;
		public int highestSavedIndex = 0;
		public long totalCentiseconds = 0;

		public string FileName = "";
		public string xmlInfo = "";
		public string sequenceInfo = "";
		public string animationInfo = "";
		public fileType sequenceType = fileType.Unrecognized;

		private struct updatedTrack
		{
			public int[] newSavedIndexes;
		}

		// CONSTRUCTOR
		//public void Sequence()
		//{ 
		//}


		public int readFile(string existingFileName)
		{
			int errState = 0;
			int pos1 = -1; // positions of certain key text in the line
			int[] trackItemCounts = new int[1];
			int itemCount4Track;

			// Zero these out from any previous run
			lineCount = 0;
			channelCount = 0;
			rgbChannelCount = 0;
			groupCount = 0;
			highestSavedIndex = 0;
			groupItemCount = 0;
			effectCount = 0;
			timingGridCount = 0;
			gridItemCount = 0;
			trackCount = 0;
			//trackItemCount = 0;
			highestSavedIndex = 0;
			totalCentiseconds = 0;





			int curChannel = -1;
			int currgbChannel = -1;
			int curSavedIndex = -1;
			int curChannelGroupList = -1;
			int curGroupItem = -1;
			int curEffect = -1;
			int curTimingGrid = -1;
			int curGridItem = -1;
			int curTrack = -1;
			int curTrackItem = -1;

			if (File.Exists(existingFileName))
			{
				StreamReader reader = new StreamReader(existingFileName);

				string lineIn; // line read in (does not get modified)

				// * PASS 1 - COUNT OBJECTS
				while ((lineIn = reader.ReadLine()) != null)
				{
					lineCount++;
					// does this line mark the start of a channel?
					pos1 = lineIn.IndexOf("xml version=");
					//if (pos1 > 0)
					//{
					//    xmlInfo = lineIn;
					//}
					pos1 = lineIn.IndexOf("saveFileVersion=");
					//if (pos1 > 0)
					//{
					//    sequenceInfo = lineIn;
					//}
					//pos1 = lineIn.IndexOf("animation rows=");
					//if (pos1 > 0)
					//{
					//    animationInfo = lineIn;
					//}
					//pos1 = lineIn.IndexOf("<channel name=");
					pos1 = lineIn.IndexOf(STFLD + TABLEchannel + SPC + FIELDname);
					if (pos1 > 0)
					{
						channelCount++;
					}
					//pos1 = lineIn.IndexOf("<rgbChannel totalCentiseconds=");
					pos1 = lineIn.IndexOf(STFLD + TABLErgbChannel + SPC);
					if (pos1 > 0)
					{
						rgbChannelCount++;
					}
					//pos1 = lineIn.IndexOf("<channelGroupList totalCentiseconds=");
					pos1 = lineIn.IndexOf(STFLD + TABLEchannelGroupList + SPC);
					if (pos1 > 0)
					{
						groupCount++;
					}
					//pos1 = lineIn.IndexOf("<effect type=");
					pos1 = lineIn.IndexOf(STFLD + TABLEeffect + SPC);
					if (pos1 > 0)
					{
						effectCount++;
					}
					if (trackCount == 0)
					{
						pos1 = lineIn.IndexOf(TABLEchannelGroup + SPC + FIELDsavedIndex);
						if (pos1 > 0)
						{
							groupItemCount++;
						}
					}
					//pos1 = lineIn.IndexOf("<timingGrid ");
					pos1 = lineIn.IndexOf(STFLD + TABLEtimingGrid + SPC);
					if (pos1 > 0)
					{
						timingGridCount++;
					}

					pos1 = lineIn.IndexOf(STFLD + TABLEtiming + SPC);
					if (pos1 > 0)
					{
						gridItemCount++;
					}

					// Tracks WITHOUT other parameters, look for just plain "<track>"
					pos1 = lineIn.IndexOf(STFLD + TABLEtrack + FINFLD);
					if (pos1 > 0)
					{
						trackCount++;
						Array.Resize(ref trackItemCounts, trackCount);
					}
					// Tracks WITH other parameters, such as name, look for "<track " (note space after 'track')
					pos1 = lineIn.IndexOf(STFLD + TABLEtrack + SPC);
					if (pos1 > 0)
					{
						trackCount++;
						Array.Resize(ref trackItemCounts, trackCount);
					}

					// Track Items
					if (trackCount > 0)
					{
						pos1 = lineIn.IndexOf(STFLD + TABLEchannel + SPC);
						if (pos1 > 0)
						{
							trackItemCount++;
						}
					}

					//pos1 = lineIn.IndexOf(" savedIndex=");
					pos1 = lineIn.IndexOf(FIELDsavedIndex);
					if (pos1 > 0)
					{
						curSavedIndex = getKeyValue(lineIn, FIELDsavedIndex);
						if (curSavedIndex > highestSavedIndex)
						{
							highestSavedIndex = curSavedIndex;
						}
					}


				}


				reader.Close();
				// CREATE ARRAYS TO HOLD OBJECTS
				//rgbChannel[] rgbChannels;
				//int[] channelGroup;
				//savedIndex[] savedIndexes;
				channels = new channel[channelCount + 2];
				savedIndexes = new savedIndex[highestSavedIndex + 3];
				rgbChannels = new rgbChannel[rgbChannelCount + 2];
				channelGroups = new channelGroupList[groupCount + 2];
				channelGroupItems = new channelGroupItem[groupItemCount + 2];
				effects = new effect[effectCount + 2];
				timingGrids = new timingGrid[timingGridCount + 2];
				timingGridItems = new timingGridItem[gridItemCount + 2];
				tracks = new track[trackCount + 2];
				trackItems = new trackItem[trackItemCount + 2];
				byte tracksSectionFound = 0;

				string sMsg;
				sMsg = "Original File Parse Complete!\r\n\r\n";
				sMsg += lineCount.ToString() + " lines\r\n";
				sMsg += channelCount.ToString() + " channels\r\n";
				sMsg += rgbChannelCount.ToString() + " RGB Channels\r\n";
				sMsg += effectCount.ToString() + " effects\r\n";
				sMsg += groupCount.ToString() + " groups\r\n";
				sMsg += groupItemCount.ToString() + " group items\r\n";
				sMsg += trackCount.ToString() + " tracks\r\n";
				sMsg += trackItemCount.ToString() + " track items in all tracks\r\n";

				DialogResult mReturn;
				//mReturn = MessageBox.Show(sMsg, "File Parse Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);


				// * PASS 2 - COLLECT OBJECTS
				reader = new StreamReader(existingFileName);
				while ((lineIn = reader.ReadLine()) != null)
				{
					lineCount++;
					// does this line mark the start of the file and have the xml info?
					pos1 = lineIn.IndexOf(XMLversion);
					if (pos1 > 0) xmlInfo = lineIn;

					// does this line mark the start of a channel config file?
					pos1 = lineIn.IndexOf(STFLD + FILEchannelConfig);
					if (pos1 >= 0)
					{
						sequenceInfo = lineIn;
						sequenceType = fileType.ChannelConfig;
					}

					// does this line mark the start of a channel config file?
					pos1 = lineIn.IndexOf(STFLD + FILEsequence);
					if (pos1 > 0)
					{
						sequenceInfo = lineIn;
						pos1 = lineIn.IndexOf(FIELDmusicFilename);
						if (pos1 > 0)
						{
							sequenceType = fileType.Musical;
						}
						else
						{
							sequenceType = fileType.Animated;
						}
					}

					// does this line mark the start of a regular channel?
					pos1 = lineIn.IndexOf(TABLEchannel + " " + FIELDname);
					if (pos1 > 0)
					{
						curChannel++;
						channel chan = new channel();
						savedIndex si = new savedIndex();
						chan.name = getKeyWord(lineIn, FIELDname);
						chan.color = getKeyValue(lineIn, FIELDcolor);
						chan.centiseconds = getKeyValue(lineIn, FIELDcentiseconds);
						chan.deviceType = enumDevice(getKeyWord(lineIn, FIELDdeviceType));
						chan.unit = getKeyValue(lineIn, FIELDunit);
						chan.network = getKeyValue(lineIn, FIELDnetwork);
						chan.circuit = getKeyValue(lineIn, FIELDcircuit);
						chan.savedIndex = getKeyValue(lineIn, FIELDsavedIndex);
						channels[curChannel] = chan;
						curSavedIndex = chan.savedIndex;

						si.objType = tableType.channel;
						si.objIndex = curChannel;
						savedIndexes[curSavedIndex] = si;

					}

					// does this line mark the start of a RGB channel?
					pos1 = lineIn.IndexOf(STFLD + TABLErgbChannel + SPC);
					if (pos1 > 0)
					{
						currgbChannel++;
						rgbChannel rgbc = new rgbChannel();
						savedIndex si = new savedIndex();
						rgbc.name = getKeyWord(lineIn, FIELDname);
						rgbc.totalCentiseconds = getKeyValue(lineIn, FIELDtotalCentiseconds);
						rgbc.savedIndex = getKeyValue(lineIn, FIELDsavedIndex);
						curSavedIndex = rgbc.savedIndex;
						lineIn = reader.ReadLine();
						lineIn = reader.ReadLine();
						rgbc.redSavedIndex = getKeyValue(lineIn, FIELDsavedIndex);
						rgbc.redChannelIndex = savedIndexes[rgbc.redSavedIndex].objIndex;
						lineIn = reader.ReadLine();
						rgbc.grnSavedIndex = getKeyValue(lineIn, FIELDsavedIndex);
						rgbc.grnChannelIndex = savedIndexes[rgbc.grnSavedIndex].objIndex;
						lineIn = reader.ReadLine();
						rgbc.bluSavedIndex = getKeyValue(lineIn, FIELDsavedIndex);
						rgbc.bluChannelIndex = savedIndexes[rgbc.bluSavedIndex].objIndex;
						rgbChannels[currgbChannel] = rgbc;

						si.objType = tableType.rgbChannel;
						si.objIndex = currgbChannel;
						savedIndexes[curSavedIndex] = si;

					}

					// does this line mark the start of a Channel Group List?
					//if (curTrack < 0)
					//{
					pos1 = lineIn.IndexOf(STFLD + TABLEchannelGroupList);
					if (pos1 > 0)
					{
						curChannelGroupList++;
						channelGroupList changl = new channelGroupList();
						savedIndex si = new savedIndex();
						changl.name = getKeyWord(lineIn, FIELDname);
						changl.channelIndex = curChannel;
						changl.channelSavedIndex = curSavedIndex;
						changl.totalCentiSeconds = getKeyValue(lineIn, FIELDtotalCentiseconds);
						changl.savedIndex = getKeyValue(lineIn, FIELDsavedIndex);
						curSavedIndex = changl.savedIndex;
						channelGroups[curChannelGroupList] = changl;

						si.objType = tableType.channelGroupList;
						si.objIndex = curChannelGroupList;
						savedIndexes[curSavedIndex] = si;
						lineIn = reader.ReadLine();
						lineIn = reader.ReadLine();
						pos1 = lineIn.IndexOf(TABLEchannelGroup + " " + FIELDsavedIndex);
						while (pos1 > 0)
						{
							curGroupItem++;
							channelGroupItem gi = new channelGroupItem();
							gi.channelGroupListIndex = curChannelGroupList;
							//gi.channelGroupSavedIndex = curSavedIndex;
							gi.channelGroupSavedIndex = getKeyValue(lineIn, FIELDsavedIndex);
							channelGroupItems[curGroupItem] = gi;

							lineIn = reader.ReadLine();
							pos1 = lineIn.IndexOf(TABLEchannelGroup + " " + FIELDsavedIndex);
						}
					}
					//}

					// does this line mark the start of an Effect?
					pos1 = lineIn.IndexOf(TABLEeffect + " " + FIELDtype);
					if (pos1 > 0)
					{
						curEffect++;

						//DEBUG!
						if (curEffect > 638)
						{
							errState = 1;
						}

						effect ef = new effect();
						ef.channelIndex = curChannel;
						ef.savedIndex = curSavedIndex;
						ef.type = enumEffect(getKeyWord(lineIn, FIELDtype));
						ef.startCentisecond = getKeyValue(lineIn, FIELDstartCentisecond);
						ef.endCentisecond = getKeyValue(lineIn, FIELDendCentisecond);
						ef.intensity = getKeyValue(lineIn, SPC + FIELDintensity);
						ef.startIntensity = getKeyValue(lineIn, FIELDstartIntensity);
						ef.endIntensity = getKeyValue(lineIn, FIELDendIntensity);
						if (channels[curChannel].firstEffectIndex < 0)
						{
							channels[curChannel].firstEffectIndex = curEffect;
						}
						channels[curChannel].finalEffectIndex = curEffect;
						effects[curEffect] = ef;
					}

					// does this line mark the start of a Timing Grid?
					pos1 = lineIn.IndexOf(STFLD + TABLEtimingGrid + " ");
					if (pos1 > 0)
					{
						curTimingGrid++;
						timingGrid tg = new timingGrid();
						tg.name = getKeyWord(lineIn, FIELDname);
						tg.type = enumGridType(getKeyWord(lineIn, FIELDtype));
						tg.saveID = getKeyValue(lineIn, FIELDsaveID);
						tg.spacing = getKeyValue(lineIn, FIELDspacing);
						timingGrids[curTimingGrid] = tg;

						if (tg.type == timingGridType.freeform)
						{
							lineIn = reader.ReadLine();
							pos1 = lineIn.IndexOf(TABLEtiming + " " + FIELDcentisecond);
							while (pos1 > 0)
							{
								curGridItem++;
								timingGridItem grit = new timingGridItem();
								grit.gridIndex = curTimingGrid;
								grit.centisecond = getKeyValue(lineIn, FIELDcentisecond);
								timingGridItems[curGridItem] = grit;

								lineIn = reader.ReadLine();
								pos1 = lineIn.IndexOf(TABLEtiming + " " + FIELDcentisecond);
							}
						}
					}

					// does this line mark the start of the Tracks section?
					pos1 = lineIn.IndexOf(STFLD + TABLEtrack + PLURAL );
					if (pos1 > 0)
					{
						tracksSectionFound = 1;  // We are now in the tracks section
						lineIn = reader.ReadLine();
					}
					if (tracksSectionFound == 1) // are we in, but not done with- the tracks section?
					{
						pos1 = lineIn.IndexOf(FINTBL + TABLEtrack + PLURAL); // does this line mark the end of the tracks section
						if (pos1 > 0)
						{
							tracksSectionFound = 2; // we are now done with the tracks section
							lineIn = reader.ReadLine();
						}
						// does this line mark the start of a Track?
						pos1 = lineIn.IndexOf(STFLD + TABLEtrack);
						while (pos1 > 0)
						{
							curTrack++;
							itemCount4Track = 0;
							track trk = new track();
							trk.name = getKeyWord(lineIn, FIELDname);
							trk.totalCentiseconds = getKeyValue(lineIn, FIELDtotalCentiseconds);
							totalCentiseconds = trk.totalCentiseconds;
							trk.timingGridSaveID = getKeyValue(lineIn, TABLEtimingGrid);

							lineIn = reader.ReadLine();
							lineIn = reader.ReadLine();
							pos1 = lineIn.IndexOf(TABLEchannel + " " + FIELDsavedIndex);
							while (pos1 > 0)
							{
								curTrackItem++;
								itemCount4Track++;
								trackItem tit = new trackItem();
								tit.trackIndex = curTrack;
								tit.savedIndex = getKeyValue(lineIn, FIELDsavedIndex);
								trackItems[curTrackItem] = tit;
								lineIn = reader.ReadLine();
								pos1 = lineIn.IndexOf(TABLEchannel + " " + FIELDsavedIndex);
							}
							trk.trackItemCount = itemCount4Track;
							tracks[curTrack] = trk;
						}
						//lineIn = reader.ReadLine();
						//lineIn = reader.ReadLine();
						//pos1 = lineIn.IndexOf(FINTBL + TABLEtrack + FINFLD);
						//while (pos1 < 0)
						//{
						//lineIn = reader.ReadLine();


					} // Found start of tracks section
				}

				reader.Close();

				channel chan2 = new channel();
				channels[curChannel + 1] = chan2;
				rgbChannel rgbc2 = new rgbChannel();
				rgbChannels[currgbChannel + 1] = rgbc2;
				savedIndex si2 = new savedIndex();
				savedIndexes[curSavedIndex + 1] = si2;
				channelGroupList changl2 = new channelGroupList();
				channelGroups[curChannelGroupList + 1] = changl2;
				channelGroupItem gi2 = new channelGroupItem();
				channelGroupItems[curGroupItem + 1] = gi2;
				effect ef2 = new effect();
				effects[curEffect + 1] = ef2;
				timingGrid tg2 = new timingGrid();
				timingGrids[curTimingGrid + 1] = tg2;
				timingGridItem grit2 = new timingGridItem();
				timingGridItems[curGridItem + 1] = grit2;
				track trk2 = new track();
				tracks[curTrack + 1] = trk2;
				trackItem tit2 = new trackItem();
				//trackItems[curTrackItem + 1] = tit2;


				totalCentiseconds = tracks[0].totalCentiseconds;


				if (errState <= 0)
				{
					FileName = existingFileName;
				}
			}
			else
			{
				errState = 1;
			}
			return errState;
		}


		public int WriteFile(string newFilename)
		{
			int errState = 0;

			//backupFile(fileName);

			string tmpFile = newFilename + ".tmp";

			writer = new StreamWriter(tmpFile);
			string lineOut = ""; // line to be written out, gets modified if necessary
													 //int pos1 = -1; // positions of certain key text in the line

			int curChannel = 0;
			int currgbChannel = 0;
			int curSavedIndex = 1;
			int curChannelGroupList = 0;
			int curGroupItem = 0;
			int curEffect = 0;
			int curTimingGrid = 0;
			int curGridItem = 0;
			int curTrack = 0;
			int curTrackItem = 0;
			bool closeChannel = false;

			lineOut = xmlInfo;
			writer.WriteLine(lineOut);
			lineOut = sequenceInfo;
			writer.WriteLine(lineOut);
			lineOut = LEVEL1 + STFLD + TABLEchannel + PLURAL + FINFLD;
			writer.WriteLine(lineOut);

			while (curChannel < channelCount)
			{
				closeChannel = false;
				lineOut = LEVEL2 + STFLD + TABLEchannel;
				lineOut += SPC + FIELDname + FIELDEQ + channels[curChannel].name + ENDQT;
				lineOut += SPC + FIELDcolor + FIELDEQ + channels[curChannel].color.ToString() + ENDQT;
				if (channels[curChannel].centiseconds > 2) lineOut += SPC + FIELDcentiseconds + FIELDEQ + channels[curChannel].centiseconds.ToString() + ENDQT;
				if (channels[curChannel].deviceType == deviceType.LOR)
				{
					lineOut += SPC + FIELDdeviceType + FIELDEQ + deviceName(channels[curChannel].deviceType) + ENDQT;
					lineOut += SPC + FIELDunit + FIELDEQ + channels[curChannel].unit.ToString() + ENDQT;
					lineOut += SPC + FIELDcircuit + FIELDEQ + channels[curChannel].circuit.ToString() + ENDQT;
				}
				else if (channels[curChannel].deviceType == deviceType.DMX)
				{
					lineOut += SPC + FIELDdeviceType + FIELDEQ + deviceName(channels[curChannel].deviceType) + ENDQT;
					lineOut += SPC + FIELDcircuit + FIELDEQ + channels[curChannel].circuit.ToString() + ENDQT;
					lineOut += SPC + FIELDnetwork + FIELDEQ + channels[curChannel].network.ToString() + ENDQT;
				}
				lineOut += SPC + FIELDsavedIndex + FIELDEQ + channels[curChannel].savedIndex.ToString() + ENDQT;
				curSavedIndex = channels[curChannel].savedIndex;
				// Are there any effects for this channel?
				//if (effects[curEffect].channelIndex == curChannel)
				if (effects[curEffect].channelIndex == curChannel)
				{
					// complete channel line with regular '>' then do effects
					lineOut += FINFLD;
					writer.WriteLine(lineOut);


					// DEBUG!
					if (curEffect > 633)
					{
						lineOut = "BREAK!";
					}
					if (curSavedIndex == 1298)
					{
						lineOut = "BREAK!";
					}


					//while (effects[curEffect].channelIndex == curChannel)
					while (effects[curEffect].channelIndex == curChannel)
					{
						closeChannel = true;
						lineOut = LEVEL3 + STFLD + TABLEeffect;
						lineOut += SPC + FIELDtype + FIELDEQ + effectName(effects[curEffect].type) + ENDQT;
						lineOut += SPC + FIELDstartCentisecond + FIELDEQ + effects[curEffect].startCentisecond.ToString() + ENDQT;
						lineOut += SPC + FIELDendCentisecond + FIELDEQ + effects[curEffect].endCentisecond.ToString() + ENDQT;
						if (effects[curEffect].intensity > -1)
						{
							lineOut += SPC + FIELDintensity + FIELDEQ + effects[curEffect].intensity.ToString() + ENDQT;
						}
						if (effects[curEffect].startIntensity > -1)
						{
							lineOut += SPC + FIELDstartIntensity + FIELDEQ + effects[curEffect].startIntensity.ToString() + ENDQT;
							lineOut += SPC + FIELDendIntensity + FIELDEQ + effects[curEffect].endIntensity.ToString() + ENDQT;

						}
						lineOut += ENDFLD;
						writer.WriteLine(lineOut);
						curEffect++;
					}
				}
				else // NO effects for this channal
				{
					// complete channel line with field end '/>'
					lineOut += ENDFLD;
					writer.WriteLine(lineOut);
				}
				if (closeChannel)
				{
					lineOut = LEVEL2 + FINTBL + TABLEchannel + FINFLD;
					writer.WriteLine(lineOut);
				}
				// Was this an RGB Channel?
				if (rgbChannels[currgbChannel].bluSavedIndex == curSavedIndex)
				{
					lineOut = LEVEL2 + STFLD + TABLErgbChannel;
					if (rgbChannels[currgbChannel].totalCentiseconds > 2) lineOut += SPC + FIELDtotalCentiseconds + FIELDEQ + rgbChannels[currgbChannel].totalCentiseconds.ToString() + ENDQT;
					lineOut += SPC + FIELDname + FIELDEQ + rgbChannels[currgbChannel].name + ENDQT;
					lineOut += SPC + FIELDsavedIndex + FIELDEQ + rgbChannels[currgbChannel].savedIndex.ToString() + ENDQT;
					lineOut += FINFLD;
					writer.WriteLine(lineOut);
					lineOut = LEVEL3 + STFLD + TABLEchannel + PLURAL + FINFLD;
					writer.WriteLine(lineOut);
					lineOut = LEVEL4 + STFLD + TABLEchannel;
					lineOut += SPC + FIELDsavedIndex + FIELDEQ + rgbChannels[currgbChannel].redSavedIndex.ToString() + ENDQT;
					lineOut += ENDFLD;
					writer.WriteLine(lineOut);
					lineOut = LEVEL4 + STFLD + TABLEchannel;
					lineOut += SPC + FIELDsavedIndex + FIELDEQ + rgbChannels[currgbChannel].grnSavedIndex.ToString() + ENDQT;
					lineOut += ENDFLD;
					writer.WriteLine(lineOut);
					lineOut = LEVEL4 + STFLD + TABLEchannel;
					lineOut += SPC + FIELDsavedIndex + FIELDEQ + rgbChannels[currgbChannel].bluSavedIndex.ToString() + ENDQT;
					lineOut += ENDFLD;
					writer.WriteLine(lineOut);
					lineOut = LEVEL3 + FINTBL + TABLEchannel + PLURAL + FINFLD;
					writer.WriteLine(lineOut);
					lineOut = LEVEL2 + FINTBL + TABLErgbChannel + FINFLD;
					writer.WriteLine(lineOut);

					curSavedIndex = rgbChannels[currgbChannel].savedIndex;
					currgbChannel++;
				}

				// is a group coming up next?

				if (curChannelGroupList < groupCount)
				{
					while ((curChannelGroupList < groupCount) && (channelGroups[curChannelGroupList].savedIndex == curSavedIndex + 1))
					{
						lineOut = LEVEL2 + STFLD + TABLEchannelGroupList;
						if (channelGroups[curChannelGroupList].totalCentiSeconds > 2) lineOut += SPC + FIELDtotalCentiseconds + FIELDEQ + channelGroups[curChannelGroupList].totalCentiSeconds.ToString() + ENDQT;
						lineOut += SPC + FIELDname + FIELDEQ + channelGroups[curChannelGroupList].name + ENDQT;
						lineOut += SPC + FIELDsavedIndex + FIELDEQ + channelGroups[curChannelGroupList].savedIndex.ToString() + ENDQT;
						lineOut += FINFLD;
						writer.WriteLine(lineOut);
						lineOut = LEVEL3 + STFLD + TABLEchannelGroup + PLURAL + FINFLD;
						writer.WriteLine(lineOut);
						curGroupItem = 0;
						while (curGroupItem < groupItemCount )
						{
							if (channelGroupItems[curGroupItem].channelGroupListIndex == curChannelGroupList)
							{
								lineOut = LEVEL4 + STFLD + TABLEchannelGroup;
								lineOut += SPC + FIELDsavedIndex + FIELDEQ + channelGroupItems[curGroupItem].channelGroupSavedIndex.ToString() + ENDQT;
								lineOut += ENDFLD;
								writer.WriteLine(lineOut);
							}
							curGroupItem++;
						}
						lineOut = LEVEL3 + FINTBL + TABLEchannelGroup + PLURAL + FINFLD;
						writer.WriteLine(lineOut);
						lineOut = LEVEL2 + FINTBL + TABLEchannelGroupList + FINFLD;
						writer.WriteLine(lineOut);
						curSavedIndex = channelGroups[curChannelGroupList].savedIndex;
						curChannelGroupList++;

					}
				}

				curChannel++;
			} // curChannel < channelCount

			lineOut = LEVEL1 + FINTBL + TABLEchannel + PLURAL + FINFLD;
			writer.WriteLine(lineOut);


			// TIMING GRIDS
			if (sequenceType != fileType.ChannelConfig)
			{
				if (timingGridCount > 0)
				{
					lineOut = LEVEL1 + STFLD + TABLEtimingGrid + PLURAL + FINFLD;
					writer.WriteLine(lineOut);
					while (curTimingGrid < timingGridCount)
					{
						lineOut = LEVEL2 + STFLD + TABLEtimingGrid;
						lineOut += SPC + FIELDsaveID + FIELDEQ + timingGrids[curTimingGrid].saveID.ToString() + ENDQT;
						if (timingGrids[curTimingGrid].name.Length > 1)
						{
							lineOut += SPC + FIELDname + FIELDEQ + timingGrids[curTimingGrid].name + ENDQT;
						}
						lineOut += SPC + FIELDtype + FIELDEQ + timingName(timingGrids[curTimingGrid].type) + ENDQT;
						if (timingGrids[curTimingGrid].spacing > 1)
						{
							lineOut += SPC + FIELDspacing + FIELDEQ + timingGrids[curTimingGrid].spacing.ToString() + ENDQT;
						}
						if (timingGrids[curTimingGrid].type == timingGridType.GRfixed)
						{
							lineOut += ENDFLD;
							writer.WriteLine(lineOut);
						}
						else if (timingGrids[curTimingGrid].type == timingGridType.freeform)
						{
							lineOut += FINFLD;
							writer.WriteLine(lineOut);

							while (timingGridItems[curGridItem].gridIndex == curTimingGrid)
							{
								lineOut = LEVEL4 + STFLD + TABLEtiming;
								if (timingGridItems[curGridItem].centisecond > 0) lineOut += SPC + FIELDcentisecond + FIELDEQ + timingGridItems[curGridItem].centisecond.ToString() + ENDQT;
								lineOut += ENDFLD;
								writer.WriteLine(lineOut);

								curGridItem++;
							}

							lineOut = LEVEL2 + FINTBL + TABLEtimingGrid + FINFLD;
							writer.WriteLine(lineOut);

						}
						curTimingGrid++;
					}
					lineOut = LEVEL1 + FINTBL + TABLEtimingGrid + PLURAL + FINFLD;
					writer.WriteLine(lineOut);
				}
			}


			// TRACKS
			lineOut = LEVEL1 + STFLD + TABLEtrack + PLURAL + FINFLD;
			writer.WriteLine(lineOut);
			while (curTrack < trackCount)
			{
				lineOut = LEVEL2 + STFLD + TABLEtrack;
				if (tracks[curTrack].name.Length > 1)
				{
					lineOut += SPC + FIELDname + FIELDEQ + tracks[curTrack].name + ENDQT;
				}
				if (tracks[curTrack].totalCentiseconds > 2) lineOut += SPC + FIELDtotalCentiseconds + FIELDEQ + tracks[curTrack].totalCentiseconds.ToString() + ENDQT;
				if (tracks[curTrack].timingGridIndex > 0) lineOut += SPC + TABLEtimingGrid + FIELDEQ + tracks[curTrack].timingGridSaveID.ToString() + ENDQT;
				lineOut += FINFLD;
				writer.WriteLine(lineOut);

				lineOut = LEVEL3 + STFLD + TABLEchannel + PLURAL + FINFLD;
				writer.WriteLine(lineOut);

				for (int curItem = 0; curItem < trackItemCount; curItem++)
				{
					if (trackItems[curItem] != null)
					{
						if (trackItems[curItem].trackIndex == curTrack)
						{
							lineOut = LEVEL4 + STFLD + TABLEchannel;
							lineOut += SPC + FIELDsavedIndex + FIELDEQ + trackItems[curItem].savedIndex.ToString() + ENDQT;
							lineOut += ENDFLD;
							writer.WriteLine(lineOut);
						} // track match
					} // not null
				} // for loop

				lineOut = LEVEL3 + FINTBL + TABLEchannel + PLURAL + FINFLD;
				writer.WriteLine(lineOut);
				curTrack++;

				if (sequenceType == fileType.Animated)
				{
					// TODO: Support Loops!
					lineOut = LEVEL2 + STFLD + "loopLevels/>";
					writer.WriteLine(lineOut);
				}

				lineOut = LEVEL2 + FINTBL + TABLEtrack + FINFLD;
				writer.WriteLine(lineOut);

			}
			lineOut = LEVEL1 + FINTBL + TABLEtrack + PLURAL + FINFLD;
			writer.WriteLine(lineOut);

			//lineOut = animationInfo;
			//writer.WriteLine(lineOut);

			if (sequenceType == fileType.ChannelConfig)
			{
				lineOut = FINTBL + FILEchannelConfig + FINFLD; //"</sequence>";
			}
			else
			{
				lineOut = FINTBL + FILEsequence + FINFLD; //"</sequence>";
			}
			writer.WriteLine(lineOut);



			Console.WriteLine(lineCount.ToString() + " Out:" + lineOut);
			Console.WriteLine("");


			writer.Flush();
			writer.Close();

			if (errState <= 0)
			{
				FileName = tmpFile;
			}

			if (File.Exists(newFilename))
			{
				File.Delete(newFilename);
			}
			File.Move(tmpFile, newFilename);

			return errState;
		}

		public int WriteFileInDisplayOrder(string newFilename)
		{
			int errState = 0;

			//backupFile(fileName);

			string tmpFile = newFilename;

			writer = new StreamWriter(tmpFile);
			lineOut = ""; // line to be written out, gets modified if necessary
										//int pos1 = -1; // positions of certain key text in the line

			int curTimingGrid = 0;
			int curGridItem = 0;
			int curTrack = 0;
			int curTrackItem = 0;
			int[] newSIs = new int[1];
			int newSI = -1;
			updatedTrack[] updatedTracks = new updatedTrack[trackCount];

			lineOut = xmlInfo;
			writer.WriteLine(lineOut);
			lineOut = sequenceInfo;
			writer.WriteLine(lineOut);

			lineOut = LEVEL1 + STFLD + TABLEchannel + PLURAL + FINFLD;
			writer.WriteLine(lineOut);

			for (int t = 0; t < trackCount; t++)
			{
				newSIs = writeTrackItems(t);
				updatedTrack ut = new updatedTrack();
				ut.newSavedIndexes = newSIs;
				updatedTracks[t] = ut;
			}

			lineOut = LEVEL1 + FINTBL + TABLEchannel + PLURAL + FINFLD;
			writer.WriteLine(lineOut);

			// TIMING GRIDS
			lineOut = LEVEL1 + STFLD + TABLEtimingGrid + PLURAL + FINFLD;
			writer.WriteLine(lineOut);
			while (curTimingGrid < timingGridCount)
			{
				lineOut = LEVEL2 + STFLD + TABLEtimingGrid;
				lineOut += SPC + FIELDsaveID + FIELDEQ + timingGrids[curTimingGrid].saveID.ToString() + ENDQT;
				if (timingGrids[curTimingGrid].name.Length > 1)
				{
					lineOut += SPC + FIELDname + FIELDEQ + timingGrids[curTimingGrid].name + ENDQT;
				}
				lineOut += SPC + FIELDtype + FIELDEQ + timingName(timingGrids[curTimingGrid].type) + ENDQT;
				if (timingGrids[curTimingGrid].spacing > 1)
				{
					lineOut += SPC + FIELDspacing + FIELDEQ + timingGrids[curTimingGrid].spacing.ToString() + ENDQT;
				}
				if (timingGrids[curTimingGrid].type == timingGridType.GRfixed)
				{
					lineOut += ENDFLD;
					writer.WriteLine(lineOut);
				}
				else if (timingGrids[curTimingGrid].type == timingGridType.freeform)
				{
					lineOut += FINFLD;
					writer.WriteLine(lineOut);

					while (timingGridItems[curGridItem].gridIndex == curTimingGrid)
					{
						lineOut = LEVEL4 + STFLD + TABLEtiming;
						if (timingGridItems[curGridItem].centisecond > 0) lineOut += SPC + FIELDcentisecond + FIELDEQ + timingGridItems[curGridItem].centisecond.ToString() + ENDQT;
						lineOut += ENDFLD;
						writer.WriteLine(lineOut);

						curGridItem++;
					}

					lineOut = LEVEL2 + FINTBL + TABLEtimingGrid + FINFLD;
					writer.WriteLine(lineOut);

				}
				curTimingGrid++;
			}
			lineOut = LEVEL1 + FINTBL + TABLEtimingGrid + PLURAL + FINFLD;
			writer.WriteLine(lineOut);


			// TRACKS
			lineOut = LEVEL1 + STFLD + TABLEtrack + PLURAL + FINFLD;
			writer.WriteLine(lineOut);
			while (curTrack < trackCount)
			{
				lineOut = LEVEL2 + STFLD + TABLEtrack;
				if (tracks[curTrack].name.Length > 1)
				{
					lineOut += SPC + FIELDname + FIELDEQ + tracks[curTrack].name + ENDQT;
				}
				if (tracks[curTrack].totalCentiseconds > 2) lineOut += SPC + FIELDtotalCentiseconds + FIELDEQ + tracks[curTrack].totalCentiseconds.ToString() + ENDQT;
				lineOut += SPC + TABLEtimingGrid + FIELDEQ + tracks[curTrack].timingGridSaveID.ToString() + ENDQT;
				lineOut += FINFLD;
				writer.WriteLine(lineOut);

				lineOut = LEVEL3 + STFLD + TABLEchannel + PLURAL + FINFLD;
				writer.WriteLine(lineOut);

				while (trackItems[curTrackItem].trackIndex == curTrack)
				{
					lineOut = LEVEL4 + STFLD + TABLEchannel;
					//lineOut += SPC + FIELDsavedIndex + FIELDEQ + trackItems[curTrackItem].savedIndex.ToString() + ENDQT;
					newSI = updatedTracks[curTrack].newSavedIndexes[curTrackItem];
					lineOut += SPC + FIELDsavedIndex + FIELDEQ + newSI.ToString() + ENDQT;
					lineOut += ENDFLD;
					writer.WriteLine(lineOut);

					curTrackItem++;
				}

				lineOut = LEVEL3 + FINTBL + TABLEchannel + PLURAL + FINFLD;
				writer.WriteLine(lineOut);
				curTrack++;

				// TODO: Support Loops!
				lineOut = LEVEL2 + STFLD + "loopLevels/>";
				writer.WriteLine(lineOut);

				lineOut = LEVEL2 + FINTBL + TABLEtrack + FINFLD;
				writer.WriteLine(lineOut);

			}
			lineOut = LEVEL1 + FINTBL + TABLEtrack + PLURAL + FINFLD;
			writer.WriteLine(lineOut);

			lineOut = animationInfo;
			writer.WriteLine(lineOut);

			lineOut = "</sequence>";
			writer.WriteLine(lineOut);



			Console.WriteLine(lineCount.ToString() + " Out:" + lineOut);
			Console.WriteLine("");


			writer.Flush();
			writer.Close();

			if (errState <= 0)
			{
				FileName = tmpFile;
			}

			return errState;
		}

		private int[] writeTrackItems(int trackIndex)
		{
			int saveIndex = -1;
			int itemCount = 0;
			int[] newSIs = new int[1];
			track tr = tracks[trackIndex];
			//int curTrackItem = findFirstTrackItem(trackIndex);
			//while (trackItems[curTrackItem].trackIndex == trackIndex)
			for (int curTrackItem = 0; curTrackItem < trackItemCount; curTrackItem++)
			{
				if (trackItems[curTrackItem].trackIndex == trackIndex)
				{
					int si = trackItems[curTrackItem].savedIndex;
					if (savedIndexes[si] != null)
					{
						if (savedIndexes[si].objType == tableType.channel)
						{
							saveIndex = writeChannel(savedIndexes[si].objIndex);
						}
						else if (savedIndexes[si].objType == tableType.rgbChannel)
						{
							saveIndex = writergbChannel(savedIndexes[si].objIndex);
						}
						else if (savedIndexes[si].objType == tableType.channelGroupList)
						{
							saveIndex = writeChannelGroup(savedIndexes[si].objIndex);
						}
						Array.Resize(ref newSIs, itemCount + 1);
						newSIs[itemCount] = saveIndex;
						itemCount++;
						curTrackItem++;
					}
				}
			}
			return newSIs;
		}

		private int writeChannel(int channelIndex)
		{
			if (!channels[channelIndex].written)
			{
				int firstEffect = -1;

				lineOut = LEVEL2 + STFLD + TABLEchannel;
				lineOut += SPC + FIELDname + FIELDEQ + channels[channelIndex].name + ENDQT;
				lineOut += SPC + FIELDcolor + FIELDEQ + channels[channelIndex].color.ToString() + ENDQT;
				if (channels[channelIndex].centiseconds > 2) lineOut += SPC + FIELDcentiseconds + FIELDEQ + channels[channelIndex].centiseconds.ToString() + ENDQT;
				if (channels[channelIndex].deviceType == deviceType.LOR)
				{
					lineOut += SPC + FIELDdeviceType + FIELDEQ + deviceName(channels[channelIndex].deviceType) + ENDQT;
					lineOut += SPC + FIELDunit + FIELDEQ + channels[channelIndex].unit.ToString() + ENDQT;
					lineOut += SPC + FIELDcircuit + FIELDEQ + channels[channelIndex].circuit.ToString() + ENDQT;
				}
				else if (channels[channelIndex].deviceType == deviceType.DMX)
				{
					lineOut += SPC + FIELDdeviceType + FIELDEQ + deviceName(channels[channelIndex].deviceType) + ENDQT;
					lineOut += SPC + FIELDcircuit + FIELDEQ + channels[channelIndex].circuit.ToString() + ENDQT;
					lineOut += SPC + FIELDnetwork + FIELDEQ + channels[channelIndex].network.ToString() + ENDQT;
				}
				lineOut += SPC + FIELDsavedIndex + FIELDEQ + curSavedIndex.ToString() + ENDQT;
				channels[channelIndex].savedIndex = curSavedIndex;
				curSavedIndex++;

				// Are there any effects for this channel?
				firstEffect = findFirstEffect(channelIndex);
				if (firstEffect >= 0)
				{
					// complete channel line with regular '>' then do effects
					lineOut += FINFLD;
					writer.WriteLine(lineOut);

					writeEffects(channelIndex, firstEffect);

					lineOut = LEVEL2 + FINTBL + TABLEchannel + FINFLD;
					writer.WriteLine(lineOut);
				}
				else // NO effects for this channal
				{
					// complete channel line with field end '/>'
					lineOut += ENDFLD;
					writer.WriteLine(lineOut);
				}
				channels[channelIndex].written = true;
			}
			return channels[channelIndex].savedIndex;
		}


		private int writergbChannel(int rgbChannelIndex)
		{
			if (!rgbChannels[rgbChannelIndex].written)
			{
				int redSavedIndex = writeChannel(rgbChannels[rgbChannelIndex].redChannelIndex);
				int grnSavedIndex = writeChannel(rgbChannels[rgbChannelIndex].grnChannelIndex);
				int bluSavedIndex = writeChannel(rgbChannels[rgbChannelIndex].bluChannelIndex);

				lineOut = LEVEL2 + STFLD + TABLErgbChannel;
				if (rgbChannels[rgbChannelIndex].totalCentiseconds > 2) lineOut += SPC + FIELDtotalCentiseconds + FIELDEQ + rgbChannels[rgbChannelIndex].totalCentiseconds.ToString() + ENDQT;
				lineOut += SPC + FIELDname + FIELDEQ + rgbChannels[rgbChannelIndex].name + ENDQT;
				lineOut += SPC + FIELDsavedIndex + FIELDEQ + curSavedIndex.ToString() + ENDQT;
				rgbChannels[rgbChannelIndex].savedIndex = curSavedIndex;
				curSavedIndex++;
				lineOut += FINFLD;
				writer.WriteLine(lineOut);
				lineOut = LEVEL3 + STFLD + TABLEchannel + PLURAL + FINFLD;
				writer.WriteLine(lineOut);
				lineOut = LEVEL4 + STFLD + TABLEchannel;
				lineOut += SPC + FIELDsavedIndex + FIELDEQ + redSavedIndex.ToString() + ENDQT;
				lineOut += ENDFLD;
				writer.WriteLine(lineOut);
				lineOut = LEVEL4 + STFLD + TABLEchannel;
				lineOut += SPC + FIELDsavedIndex + FIELDEQ + grnSavedIndex.ToString() + ENDQT;
				lineOut += ENDFLD;
				writer.WriteLine(lineOut);
				lineOut = LEVEL4 + STFLD + TABLEchannel;
				lineOut += SPC + FIELDsavedIndex + FIELDEQ + bluSavedIndex.ToString() + ENDQT;
				lineOut += ENDFLD;
				writer.WriteLine(lineOut);
				lineOut = LEVEL3 + FINTBL + TABLEchannel + PLURAL + FINFLD;
				writer.WriteLine(lineOut);
				lineOut = LEVEL2 + FINTBL + TABLErgbChannel + FINFLD;
				writer.WriteLine(lineOut);

				rgbChannels[rgbChannelIndex].written = true;
			}
			return rgbChannels[rgbChannelIndex].savedIndex;
		}

		private int writeChannelGroup(int channelGroupIndex)
		{
			//dbgMsg = "writeChannelGroup(Index=" + channelGroupIndex.ToString() + ") SavedIndex=" + channelGroups[channelGroupIndex].savedIndex + " Name=" + channelGroups[channelGroupIndex].name   ;
			//Debug.Print(dbgMsg);

			int[] newSIs;
			int itemNo = 0;

			if (!channelGroups[channelGroupIndex].written)
			{
				int curItem = findFirstGroupItem(channelGroupIndex);

				newSIs = writeChannelGroupItems(channelGroupIndex, curItem);

				lineOut = LEVEL2 + STFLD + TABLEchannelGroupList;
				if (channelGroups[channelGroupIndex].totalCentiSeconds > 2) lineOut += SPC + FIELDtotalCentiseconds + FIELDEQ + channelGroups[channelGroupIndex].totalCentiSeconds.ToString() + ENDQT;
				lineOut += SPC + FIELDname + FIELDEQ + channelGroups[channelGroupIndex].name + ENDQT;
				lineOut += SPC + FIELDsavedIndex + FIELDEQ + curSavedIndex.ToString() + ENDQT;
				channelGroups[channelGroupIndex].savedIndex = curSavedIndex;
				curSavedIndex++;
				lineOut += FINFLD;
				writer.WriteLine(lineOut);
				lineOut = LEVEL3 + STFLD + TABLEchannelGroup + PLURAL + FINFLD;
				writer.WriteLine(lineOut);

				while (channelGroupItems[curItem].channelGroupListIndex == channelGroupIndex)
				{
					lineOut = LEVEL4 + STFLD + TABLEchannelGroup;
					lineOut += SPC + FIELDsavedIndex + FIELDEQ + newSIs[itemNo].ToString() + ENDQT;
					lineOut += ENDFLD;
					writer.WriteLine(lineOut);
					itemNo++;
					curItem++;
				}

				lineOut = LEVEL3 + FINTBL + TABLEchannelGroup + PLURAL + FINFLD;
				writer.WriteLine(lineOut);
				lineOut = LEVEL2 + FINTBL + TABLEchannelGroupList + FINFLD;
				writer.WriteLine(lineOut);

			}
			return channelGroups[channelGroupIndex].savedIndex;
		}

		private int[] writeChannelGroupItems(int channelGroupIndex, int firstGroupItem)
		{
			int[] newSIs = new int[1];
			int itemCount = 0;
			int si = -1;
			int saveIndex = -1;
			int curGroupItem = firstGroupItem;

			while (channelGroupItems[curGroupItem].channelGroupListIndex == channelGroupIndex)
			{
				si = channelGroupItems[curGroupItem].channelGroupSavedIndex;
				if (savedIndexes[si].objType == tableType.channel)
				{
					saveIndex = writeChannel(savedIndexes[si].objIndex);
				}
				else if (savedIndexes[si].objType == tableType.rgbChannel)
				{
					saveIndex = writergbChannel(savedIndexes[si].objIndex);
				}
				else if (savedIndexes[si].objType == tableType.channelGroupList)
				{
					saveIndex = writeChannelGroup(savedIndexes[si].objIndex);
				}

				Array.Resize(ref newSIs, itemCount + 1);
				newSIs[itemCount] = saveIndex;
				itemCount++;


				curGroupItem++;
			}
			return newSIs;
		}

		private void writeEffects(int channelIndex, int firstEffectIndex)
		{
			int curEffect = firstEffectIndex;
			while (effects[curEffect].channelIndex == channelIndex)
			{
				lineOut = LEVEL3 + STFLD + TABLEeffect;
				lineOut += SPC + FIELDtype + FIELDEQ + effectName(effects[curEffect].type) + ENDQT;
				lineOut += SPC + FIELDstartCentisecond + FIELDEQ + effects[curEffect].startCentisecond.ToString() + ENDQT;
				lineOut += SPC + FIELDendCentisecond + FIELDEQ + effects[curEffect].endCentisecond.ToString() + ENDQT;
				if (effects[curEffect].intensity > -1)
				{
					lineOut += SPC + FIELDintensity + FIELDEQ + effects[curEffect].intensity.ToString() + ENDQT;
				}
				if (effects[curEffect].startIntensity > -1)
				{
					lineOut += SPC + FIELDstartIntensity + FIELDEQ + effects[curEffect].startIntensity.ToString() + ENDQT;
					lineOut += SPC + FIELDendIntensity + FIELDEQ + effects[curEffect].endIntensity.ToString() + ENDQT;

				}
				lineOut += ENDFLD;
				writer.WriteLine(lineOut);
				curEffect++;
			}
		}

		public static Int32 getKeyValue(string lineIn, string keyWord)
		{
			Int32 valueOut = -1;
			int pos1 = -1;
			int pos2 = -1;
			string fooo = "";

			pos1 = lineIn.IndexOf(keyWord + "=");
			if (pos1 > 0)
			{
				fooo = lineIn.Substring(pos1 + keyWord.Length + 2);
				pos2 = fooo.IndexOf("\"");
				fooo = fooo.Substring(0, pos2);
				valueOut = Convert.ToInt32(fooo);
			}
			else
			{
				valueOut = -1;
			}

			return valueOut;
		}

		public static string getKeyWord(string lineIn, string keyWord)
		{
			string valueOut = "";
			int pos1 = -1;
			int pos2 = -1;
			string fooo = "";

			pos1 = lineIn.IndexOf(keyWord + "=");
			if (pos1 > 0)
			{
				fooo = lineIn.Substring(pos1 + keyWord.Length + 2);
				pos2 = fooo.IndexOf("\"");
				fooo = fooo.Substring(0, pos2);
				valueOut = fooo;
			}
			else
			{
				valueOut = "";
			}

			return valueOut;
		}

		private static deviceType enumDevice(string deviceName)
		{
			deviceType valueOut = deviceType.None;

			if (deviceName == DEVICElor)
			{
				valueOut = deviceType.LOR;
			}
			else if (deviceName == DEVICEdmx)
			{
				valueOut = deviceType.DMX;
			}
			else if (deviceName == DEVICEdigital)
			{
				valueOut = deviceType.Digital;
			}
			else if (deviceName == "")
			{
				valueOut = deviceType.None;
			}
			else
			{
				// TODO: throw exception here!
				valueOut = deviceType.None;
				string sMsg = "Unrecognized Device Type: ";
				sMsg += deviceName;
				DialogResult dr = MessageBox.Show(sMsg, "Unrecognized Keyword", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}

			return valueOut;
		}

		private static effectType enumEffect(string effectName)
		{
			effectType valueOut = effectType.None;

			if (effectName == EFFECTintensity)
			{
				valueOut = effectType.intensity;
			}
			else if (effectName == EFFECTshimmer)
			{
				valueOut = effectType.shimmer;
			}
			else if (effectName == EFFECTtwinkle)
			{
				valueOut = effectType.twinkle;
			}
			else if (effectName == EFFECTdmx)
			{
				valueOut = effectType.DMX;
			}
			else
			{
				// TODO: throw exception here
				valueOut = effectType.None;
				string sMsg = "Unrecognized Effect Name: ";
				sMsg += effectName;
				DialogResult dr = MessageBox.Show(sMsg, "Unrecognized Keyword", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}


			return valueOut;
		}

		private static timingGridType enumGridType(string typeName)
		{
			timingGridType valueOut = timingGridType.None;

			if (typeName == GRIDfreeform)
			{
				valueOut = timingGridType.freeform;
			}
			else if (typeName == "fixed")
			{
				valueOut = timingGridType.GRfixed;
			}
			else
			{
				// TODO: throw exception here
				valueOut = timingGridType.None;
				string sMsg = "Unrecognized Timing Grid Type: ";
				sMsg += typeName;
				DialogResult dr = MessageBox.Show(sMsg, "Unrecognized Keyword", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}


			return valueOut;
		}

		private static string deviceName(deviceType devType)
		{
			string valueOut = "";
			switch (devType)
			{
				case deviceType.LOR:
					valueOut = DEVICElor;
					break;
				case deviceType.DMX:
					valueOut = DEVICEdmx;
					break;
				case deviceType.Digital:
					valueOut = DEVICEdigital;
					break;
			}
			return valueOut;
		}

		private static string effectName(effectType effType)
		{
			string valueOut = "";
			switch (effType)
			{
				case effectType.intensity:
					valueOut = EFFECTintensity;
					break;
				case effectType.shimmer:
					valueOut = EFFECTshimmer;
					break;
				case effectType.twinkle:
					valueOut = EFFECTtwinkle;
					break;
				case effectType.DMX:
					valueOut = EFFECTdmx;
					break;
			}
			return valueOut;
		}

		private static string timingName(timingGridType grdType)
		{
			string valueOut = "";
			switch (grdType)
			{
				case timingGridType.freeform:
					valueOut = GRIDfreeform;
					break;
				case timingGridType.GRfixed:
					valueOut = GRIDfixed;
					break;
			}
			return valueOut;
		}

		/*
		private int findFirstTrackItem(int trackIndex)
		{
				int firstItem = -1;
				for (int i = 0; i < trackItemCount; i++)
				{
						if (trackItems[i].trackIndex == trackIndex)
						{
								firstItem = i;
								break;
						}
				}
				return firstItem;
		}
		*/

		private int findFirstEffect(int channelIndex)
		{
			int firstItem = -1;
			for (int i = 0; i < effectCount; i++)
			{
				if (effects[i].channelIndex == channelIndex)
				{
					firstItem = i;
					break;
				}
			}
			return firstItem;
		}

		private int findFirstGroupItem(int channelGroupIndex)
		{
			int firstItem = -1;
			for (int i = 0; i < groupItemCount; i++)
			{
				if (channelGroupItems[i].channelGroupListIndex == channelGroupIndex)
				{
					firstItem = i;
					break;
				}
			}
			return firstItem;
		}
	}
}
