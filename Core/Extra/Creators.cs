﻿using Google.Protobuf;
using ICSharpCode.SharpZipLib.Zip.Compression;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;
using SharedLib.Shared;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using ZstdNet;

namespace Core
{
    public class Creators
    {

        public static string _ManifestSigner = "START_RandomBytesDoDecryptBecauseIDidntKnowHowToDecrpytTheOriginalDotManifestSignThingy_IfYourReadingThisYouUsingCustomUplayServer_RandomBytesDoDecryptBecauseIDidntKnowHowToDecrpytTheOriginalDotManifestSignThingy_IfYourReadingThisYouUsingCustomUplayServer_RandomBytesDoDecryptBecauseIDidntKnowHowToDecrpytTheOriginalDotManifestSignThingy_IfYourReadingThisYouUsingCustomUplayServer_RandomBytesDoDecryptBecauseIDidntKnowHowToDecrpytTheOriginalDotManifestSignThingy_IfYourReadingThisYouUsingCustomUplayServerFun_END";
        public static string _ManfiestB64 = "U1RBUlRfUmFuZG9tQnl0ZXNEb0RlY3J5cHRCZWNhdXNlSURpZG50S25vd0hvd1RvRGVjcnB5dFRoZU9yaWdpbmFsRG90TWFuaWZlc3RTaWduVGhpbmd5X0lmWW91clJlYWRpbmdUaGlzWW91VXNpbmdDdXN0b21VcGxheVNlcnZlcl9SYW5kb21CeXRlc0RvRGVjcnlwdEJlY2F1c2VJRGlkbnRLbm93SG93VG9EZWNycHl0VGhlT3JpZ2luYWxEb3RNYW5pZmVzdFNpZ25UaGluZ3lfSWZZb3VyUmVhZGluZ1RoaXNZb3VVc2luZ0N1c3RvbVVwbGF5U2VydmVyX1JhbmRvbUJ5dGVzRG9EZWNyeXB0QmVjYXVzZUlEaWRudEtub3dIb3dUb0RlY3JweXRUaGVPcmlnaW5hbERvdE1hbmlmZXN0U2lnblRoaW5neV9JZllvdXJSZWFkaW5nVGhpc1lvdVVzaW5nQ3VzdG9tVXBsYXlTZXJ2ZXJfUmFuZG9tQnl0ZXNEb0RlY3J5cHRCZWNhdXNlSURpZG50S25vd0hvd1RvRGVjcnB5dFRoZU9yaWdpbmFsRG90TWFuaWZlc3RTaWduVGhpbmd5X0lmWW91clJlYWRpbmdUaGlzWW91VXNpbmdDdXN0b21VcGxheVNlcnZlckZ1bl9FTkQ=";

        /// <summary>
        /// Makeing manifest file to output
        /// </summary>
        /// <param name="manifest">Manifest Data</param>
        /// <param name="FileOutput">File path (better to end with .manifest)</param>
        public static void MakeManifest(Uplay.Download.Manifest manifest, string FileOutput)
        {
            var mem = new MemoryStream();
            manifest.WriteTo(mem);
            var writer = File.OpenWrite(FileOutput);
            var binaryWriter = new BinaryWriter(writer, Encoding.UTF8, false);
            binaryWriter.Write((int)manifest.CompressionMethod);
            binaryWriter.Write(344);
            binaryWriter.Write((int)mem.Length);
            binaryWriter.Write(_ManfiestB64);
            writer.Position = 356;

            var ms = new MemoryStream();
            var defl = new Deflater(Deflater.BEST_COMPRESSION, false);
            Stream deflate = new DeflaterOutputStream(ms, defl);
            deflate.Write(mem.ToArray());
            deflate.Close();

            byte[] compressedData = ms.ToArray();
            writer.Write(compressedData);
            writer.Close();
            binaryWriter.Close();
        }

        /// <summary>
        /// Compressing File to chunks
        /// </summary>
        /// <param name="FileName">Name (or path) to a file that being to compress</param>
        /// <param name="maxSize">Chunk Max Sizes</param>
        /// <param name="compression">Compression Method</param>
        /// <param name="sliceIds">ID's to ManifestData</param>
        /// <param name="numberofpart">Number of parts (same as sliceIds.Count)</param>
        public static void CompressFile(string FileName, int maxSize, Uplay.Download.CompressionMethod compression, string savepath, string version, string prodid, out Uplay.Download.File file)
        {
            FileInfo fileinfo = new FileInfo(FileName);
            var filename = fileinfo.FullName.Replace(Path.GetDirectoryName(FileName) + "\\", "");

            file = new()
            {
                IsDir = false,
                Name = filename,
                Size = (ulong)fileinfo.Length,
                Slices = { },
                SliceList = { }
            };
            if (maxSize == 0) return; 
           
            if (new FileInfo(FileName).Length <= int.MaxValue)
            {
                File.AppendAllText("output", "File lentght is smaller than int max!");
                var slice = (File.ReadAllBytes(FileName));
                var arr = DeComp.Compress(true, true, compression.ToString(), slice, (uint)maxSize);
                File.AppendAllText("compressedfiles.txt", "UnSize: " + slice.Length + " | Compize: " + arr.Length + "\n");
                WriteOut((uint)slice.Length, arr, savepath, version, prodid, file, out var outfile);
                file = outfile;
            }
            else
            {
                File.AppendAllText("output", "File lentght is bigger than int max!");
                var sr = File.OpenRead(FileName);
                File.AppendAllText("output", "open readed!");
                byte[] buffer = new byte[maxSize];
                int bytesRead = sr.Read(buffer, 0, maxSize);
                var arr = DeComp.Compress(true, true, compression.ToString(), buffer, (uint)maxSize);
                File.AppendAllText("compressedfiles.txt", "UnSize: " + buffer.Length + " | Compize: " + arr.Length + "\n");
                WriteOut((uint)buffer.Length, arr, savepath, version, prodid, file, out var outfile);
                file = outfile;
                while (bytesRead > 0)
                {
                    //DoSomething(buffer, bytesRead);
                    bytesRead = sr.Read(buffer, 0, maxSize);
                    arr = DeComp.Compress(true, true, compression.ToString(), buffer, (uint)maxSize);
                    File.AppendAllText("compressedfiles.txt", "UnSize: " + buffer.Length + " | Compize: " + arr.Length + "\n");
                    WriteOut((uint)buffer.Length, arr, savepath, version, prodid, file, out outfile);
                    file = outfile;
                }
                sr.Dispose();

            }
        }

        public static void WriteOut(uint orisliceSize, byte[] compslice, string savepath, string version, string prodid, Uplay.Download.File file, out Uplay.Download.File outfile)
        {
            outfile = file;
            var sliceid = GenerateSliceID(compslice);
            var sliceid_b = Convert.FromHexString(sliceid);
            if (version == "3")
            {
                Uplay.Download.Slice dslice = new()
                {
                    DownloadSha1 = ByteString.CopyFrom(sliceid_b),
                    Size = orisliceSize,
                    DownloadSize = (uint)compslice.Length
                };
                file.SliceList.Add(dslice);
                var pathtowrite = $"{savepath}/Download/{prodid}/slices_v3/{Formatters.FormatSliceHashChar(sliceid)}";
                Directory.CreateDirectory(pathtowrite);
                File.WriteAllBytes(pathtowrite + "/" + sliceid, compslice);
            }
            else
            {
                var pathtowrite = $"{savepath}/Download/{prodid}/slices";
                Directory.CreateDirectory(pathtowrite);
                File.WriteAllBytes(pathtowrite + "/" + sliceid, compslice);
            }
            outfile.Slices.Add(ByteString.CopyFrom(sliceid_b));
        }

        /// <summary>
        /// Generating Slice IDs
        /// </summary>
        /// <param name="input">Bytes of Data</param>
        /// <returns>SHA1 Hash</returns>
        public static string GenerateSliceID(byte[] input)
        {
            var ret = SHA1.HashData(input);
            var returner = BitConverter.ToString(ret).Replace("-", "");
            return returner;
        }

        /// <summary>
        /// Create Achievements
        /// </summary>
        /// <param name="achId">List of Ids</param>
        /// <param name="FileName">Filename to write it</param>
        public static void CreateAch(List<uint> achId, string FileName)
        {
            Uplay.Uplaydll.AchievementsBlob achievements = new();

            foreach (var id in achId)
            {
                Uplay.Uplaydll.AchievementInBlob achievement = new();

                achievement.AchievementId = id;
                achievement.TitleId = id;
                achievement.DescriptionId = id;
                achievement.DescriptionId = id;
                achievements.Achievements.Add(achievement);
            }

            File.WriteAllBytes(FileName, achievements.ToByteArray());
        }
    }
}