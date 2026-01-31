using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
#if UNITY_STANDALONE_WIN
using System.Runtime.InteropServices;
#endif

namespace Mod
{
    public class BgmManager : MonoBehaviour
    {
        private static BgmManager _instance;
        public static BgmManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject go = new GameObject("BgmManager");
                    _instance = go.AddComponent<BgmManager>();
                    _instance.audioSource = go.AddComponent<AudioSource>();
                    _instance.audioSource.loop = true;
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        public enum PlayMode
        {
            RepeatOne,
            Sequential,
            Shuffle
        }

        private AudioSource audioSource;
        private string currentBgmFile = "";
        
        // Playlist lưu đường dẫn đầy đủ
        public List<string> playlist = new List<string>();
        public PlayMode currentMode = PlayMode.RepeatOne;

        // Windows OpenFileDialog
#if UNITY_STANDALONE_WIN
        [DllImport("Comdlg32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern bool GetOpenFileName(ref OpenFileName ofn);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct OpenFileName
        {
            public int lStructSize;
            public IntPtr hwndOwner;
            public IntPtr hInstance;
            public string lpstrFilter;
            public string lpstrCustomFilter;
            public int nMaxCustFilter;
            public int nFilterIndex;
            public IntPtr lpstrFile;  // Changed to IntPtr for output buffer
            public int nMaxFile;
            public IntPtr lpstrFileTitle;  // Changed to IntPtr
            public int nMaxFileTitle;
            public string lpstrInitialDir;
            public string lpstrTitle;
            public int Flags;
            public short nFileOffset;
            public short nFileExtension;
            public string lpstrDefExt;
            public IntPtr lCustData;
            public IntPtr lpfnHook;
            public string lpTemplateName;
            public IntPtr pvReserved;
            public int dwReserved;
            public int flagsEx;
        }
#endif

        void Awake()
        {
            LoadPlaylist();
        }

        /// <summary>
        /// Lưu playlist vào RMS
        /// </summary>
        public void SavePlaylist()
        {
            string data = string.Join("|", playlist);
            Rms.saveRMSString("bgmPlaylist", data);
        }

        /// <summary>
        /// Load playlist từ RMS
        /// </summary>
        public void LoadPlaylist()
        {
            playlist.Clear();
            string data = Rms.loadRMSString("bgmPlaylist");
            if (!string.IsNullOrEmpty(data))
            {
                string[] paths = data.Split('|');
                foreach (string path in paths)
                {
                    if (!string.IsNullOrEmpty(path) && File.Exists(path))
                    {
                        playlist.Add(path);
                    }
                }
            }
            // Nếu playlist rỗng, vẫn giữ nguyên (không tự động load từ folder)
        }

        /// <summary>
        /// Thêm nhạc vào playlist từ đường dẫn
        /// </summary>
        public bool AddMusic(string filePath)
        {
            if (string.IsNullOrEmpty(filePath)) return false;
            if (!File.Exists(filePath))
            {
                SafeAddInfo("File không tồn tại: " + filePath);
                return false;
            }

            string ext = Path.GetExtension(filePath).ToLower();
            if (ext != ".mp3" && ext != ".ogg" && ext != ".wav" && ext != ".aiff")
            {
                SafeAddInfo("Định dạng không hỗ trợ. Chỉ hỗ trợ: mp3, ogg, wav, aiff");
                return false;
            }

            if (!playlist.Contains(filePath))
            {
                playlist.Add(filePath);
                SavePlaylist();
                SafeAddInfo("Đã thêm: " + Path.GetFileName(filePath));
                return true;
            }
            else
            {
                SafeAddInfo("Bài hát đã có trong danh sách");
                return false;
            }
        }

        /// <summary>
        /// Xóa nhạc khỏi playlist
        /// </summary>
        public void RemoveMusic(string filePath)
        {
            if (playlist.Contains(filePath))
            {
                playlist.Remove(filePath);
                SavePlaylist();
                SafeAddInfo("Đã xóa: " + Path.GetFileName(filePath));
            }
        }

        /// <summary>
        /// Lấy tên file từ đường dẫn
        /// </summary>
        public string GetDisplayName(string filePath)
        {
            return Path.GetFileName(filePath);
        }

        /// <summary>
        /// Thêm tất cả file nhạc từ folder
        /// </summary>
        public int AddMusicFromFolder(string folderPath)
        {
            if (!Directory.Exists(folderPath))
            {
                SafeAddInfo("Folder không tồn tại: " + folderPath);
                return 0;
            }

            string[] extensions = { "*.mp3", "*.ogg", "*.wav", "*.aiff" };
            int addedCount = 0;
            int skippedCount = 0;

            foreach (string ext in extensions)
            {
                string[] files = Directory.GetFiles(folderPath, ext);
                foreach (string file in files)
                {
                    if (playlist.Contains(file))
                    {
                        skippedCount++;
                        continue;
                    }
                    playlist.Add(file);
                    addedCount++;
                }
            }

            if (addedCount > 0)
            {
                SavePlaylist();
            }

            if (addedCount > 0 && skippedCount > 0)
            {
                SafeAddInfo("Đã thêm " + addedCount + ", bỏ qua " + skippedCount + " bài trùng");
            }
            else if (addedCount > 0)
            {
                SafeAddInfo("Đã thêm " + addedCount + " bài nhạc");
            }
            else if (skippedCount > 0)
            {
                SafeAddInfo("Tất cả " + skippedCount + " bài đã có trong danh sách");
            }
            
            return addedCount;
        }

        /// <summary>
        /// Mở File Browser để chọn nhạc (Windows)
        /// </summary>
        public void OpenFileBrowser()
        {
#if UNITY_STANDALONE_WIN
            StartCoroutine(OpenFileBrowserCoroutine());
#else
            SafeAddInfo("Chức năng này chỉ hỗ trợ trên Windows PC");
#endif
        }

        /// <summary>
        /// Mở Folder Browser để chọn folder nhạc (Windows)
        /// </summary>
        public void OpenFolderBrowser()
        {
#if UNITY_STANDALONE_WIN
            StartCoroutine(OpenFolderBrowserCoroutine());
#else
            SafeAddInfo("Chức năng này chỉ hỗ trợ trên Windows PC");
#endif
        }

#if UNITY_STANDALONE_WIN
        // Windows Shell32 API for folder browser
        [DllImport("shell32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SHBrowseForFolder(ref BROWSEINFO bi);

        [DllImport("shell32.dll", CharSet = CharSet.Auto)]
        private static extern bool SHGetPathFromIDList(IntPtr pidl, System.Text.StringBuilder pszPath);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct BROWSEINFO
        {
            public IntPtr hwndOwner;
            public IntPtr pidlRoot;
            public string pszDisplayName;
            public string lpszTitle;
            public uint ulFlags;
            public IntPtr lpfn;
            public IntPtr lParam;
            public int iImage;
        }

        private IEnumerator OpenFileBrowserCoroutine()
        {
            yield return null; // Đợi 1 frame

            // Lưu current directory để restore sau
            string originalDir = Directory.GetCurrentDirectory();
            
            // Allocate buffer for file path
            int maxFileLength = 65535;
            IntPtr fileBuffer = Marshal.AllocHGlobal(maxFileLength * 2); // *2 for Unicode
            IntPtr titleBuffer = Marshal.AllocHGlobal(128 * 2);
            
            // Zero out buffer
            for (int i = 0; i < maxFileLength * 2; i++)
            {
                Marshal.WriteByte(fileBuffer, i, 0);
            }
            
            bool dialogResult = false;
            
            try
            {
                OpenFileName ofn = new OpenFileName();
                ofn.lStructSize = Marshal.SizeOf(ofn);
                ofn.lpstrFilter = "Audio Files\0*.mp3;*.ogg;*.wav;*.aiff\0All Files\0*.*\0";
                ofn.lpstrFile = fileBuffer;
                ofn.nMaxFile = maxFileLength;
                ofn.lpstrFileTitle = titleBuffer;
                ofn.nMaxFileTitle = 128;
                ofn.lpstrTitle = "Chọn file nhạc (giữ Ctrl để chọn nhiều)";
                // OFN_EXPLORER | OFN_FILEMUSTEXIST | OFN_ALLOWMULTISELECT | OFN_NOCHANGEDIR
                ofn.Flags = 0x00080000 | 0x00001000 | 0x00000200 | 0x00000008;

                dialogResult = GetOpenFileName(ref ofn);
            }
            catch (System.Exception ex)
            {
                Debug.LogError("[BgmManager] OpenFileDialog error: " + ex.Message);
            }
            
            // Restore directory ngay sau khi dialog đóng
            Directory.SetCurrentDirectory(originalDir);
            
            if (!dialogResult)
            {
                Marshal.FreeHGlobal(fileBuffer);
                Marshal.FreeHGlobal(titleBuffer);
                yield break;
            }
            
            int addedCount = 0;
            
            // Đọc paths từ buffer
            List<string> paths = new List<string>();
            int offset = 0;
            while (true)
            {
                string path = Marshal.PtrToStringAuto(new IntPtr(fileBuffer.ToInt64() + offset));
                if (string.IsNullOrEmpty(path))
                {
                    break;
                }
                paths.Add(path);
                offset += (path.Length + 1) * 2; // +1 for null, *2 for Unicode
            }
            
            // Free buffers
            Marshal.FreeHGlobal(fileBuffer);
            Marshal.FreeHGlobal(titleBuffer);
            
            if (paths.Count == 0)
            {
                SafeAddInfo("Không tìm thấy dữ liệu");
                yield break;
            }
            
            if (paths.Count == 1)
            {
                // Chỉ 1 file - full path
                if (AddMusic(paths[0]))
                {
                    SafeAddInfo("Đã thêm: " + Path.GetFileName(paths[0]));
                    RefreshMusicTab();
                }
            }
            else if (paths.Count > 1)
            {
                // Multi-select: paths[0] = folder, paths[1..n] = filenames
                string folder = paths[0];
                int skippedCount = 0;
                
                for (int i = 1; i < paths.Count; i++)
                {
                    string fullPath = Path.Combine(folder, paths[i]);
                    
                    if (!File.Exists(fullPath)) continue;
                    if (playlist.Contains(fullPath))
                    {
                        skippedCount++;
                        continue;
                    }
                    
                    if (AddMusicSilent(fullPath))
                    {
                        addedCount++;
                    }
                }
                
                if (addedCount > 0 && skippedCount > 0)
                {
                    SafeAddInfo("Đã thêm " + addedCount + ", bỏ qua " + skippedCount + " bài trùng");
                }
                else if (addedCount > 0)
                {
                    SafeAddInfo("Đã thêm " + addedCount + " bài nhạc");
                }
                else if (skippedCount > 0)
                {
                    SafeAddInfo("Tất cả " + skippedCount + " bài đã có");
                }
                
                if (addedCount > 0)
                {
                    RefreshMusicTab();
                }
            }
        }
        
        /// <summary>
        /// Thêm nhạc không hiển thị thông báo (dùng cho multi-add)
        /// </summary>
        private bool AddMusicSilent(string filePath)
        {
            if (string.IsNullOrEmpty(filePath)) return false;
            if (!File.Exists(filePath)) return false;

            string ext = Path.GetExtension(filePath).ToLower();
            if (ext != ".mp3" && ext != ".ogg" && ext != ".wav" && ext != ".aiff")
                return false;

            if (!playlist.Contains(filePath))
            {
                playlist.Add(filePath);
                SavePlaylist();
                return true;
            }
            return false;
        }

        private IEnumerator OpenFolderBrowserCoroutine()
        {
            yield return null; // Đợi 1 frame

            BROWSEINFO bi = new BROWSEINFO();
            bi.lpszTitle = "Chọn folder chứa nhạc";
            bi.ulFlags = 0x00000001 | 0x00000010; // BIF_RETURNONLYFSDIRS | BIF_EDITBOX

            IntPtr pidl = SHBrowseForFolder(ref bi);
            if (pidl != IntPtr.Zero)
            {
                System.Text.StringBuilder path = new System.Text.StringBuilder(260);
                if (SHGetPathFromIDList(pidl, path))
                {
                    string folderPath = path.ToString();
                    int addedCount = AddMusicFromFolder(folderPath);
                    if (addedCount > 0)
                    {
                        SafeAddInfo("Đã thêm " + addedCount + " bài nhạc từ folder");
                        RefreshMusicTab();
                    }
                    else
                    {
                        SafeAddInfo("Không tìm thấy file nhạc trong folder");
                    }
                }
                Marshal.FreeCoTaskMem(pidl);
            }
        }
#endif

        /// <summary>
        /// Refresh tab nhạc sau khi thêm/xóa bài
        /// </summary>
        public void RefreshMusicTab()
        {
            SoundMn.gI().GetStrModFunc();
            // Cập nhật currentListLength trong Panel
            if (GameCanvas.panel != null && GameCanvas.panel.type == 26)
            {
                GameCanvas.panel.currentListLength = Panel.strModFunc.Length;
                GameCanvas.panel.cmyLim = GameCanvas.panel.currentListLength * 24 - GameCanvas.panel.hScroll;
                if (GameCanvas.panel.cmyLim < 0) GameCanvas.panel.cmyLim = 0;
            }
        }

        void Update()
        {
            if (audioSource.isPlaying && audioSource.clip != null)
            {
                audioSource.volume = ModFunc.GI().bgmVolume;
                
                // Kiểm tra nếu bài hát gần hết (còn 0.1s) để chuyển bài
                if (audioSource.time >= audioSource.clip.length - 0.1f)
                {
                    OnTrackEnd();
                }
            }
        }

        private void OnTrackEnd()
        {
            if (currentMode == PlayMode.RepeatOne)
            {
                audioSource.time = 0;
                audioSource.Play();
            }
            else
            {
                PlayNext();
            }
        }

        public void PlayNext()
        {
            if (playlist.Count == 0) return;

            string nextTrack = "";
            if (currentMode == PlayMode.Shuffle)
            {
                // Shuffle: chọn ngẫu nhiên, tránh phát lại bài hiện tại nếu có nhiều hơn 1 bài
                if (playlist.Count > 1)
                {
                    do
                    {
                        nextTrack = playlist[UnityEngine.Random.Range(0, playlist.Count)];
                    } while (nextTrack == currentBgmFile && playlist.Count > 1);
                }
                else
                {
                    nextTrack = playlist[0];
                }
            }
            else // Sequential
            {
                int currentIndex = playlist.IndexOf(currentBgmFile);
                int nextIndex = (currentIndex + 1) % playlist.Count;
                nextTrack = playlist[nextIndex];
            }

            // Cập nhật currentBgm trong ModFunc
            ModFunc.GI().currentBgm = nextTrack;
            Rms.saveRMSString("currentBgm", nextTrack);
            
            PlayMusic(nextTrack);
            
            // Hiển thị tên bài đang phát
            SafeAddInfo("Đang phát: " + Path.GetFileName(nextTrack));
        }

        public void StopMusic()
        {
            if (audioSource.isPlaying) audioSource.Stop();
            currentBgmFile = "";
        }

        public void PlayMusic(string filePath)
        {
            if (!ModFunc.GI().isBgm)
            {
                StopMusic();
                return;
            }

            if (string.IsNullOrEmpty(filePath)) return;
            
            if (File.Exists(filePath))
            {
                currentBgmFile = filePath;
                ModFunc.GI().currentBgm = filePath;
                Rms.saveRMSString("currentBgm", filePath);
                
                StopAllCoroutines();
                StartCoroutine(LoadAndPlayAudio(filePath));
            }
            else
            {
                SafeAddInfo("File không tồn tại: " + Path.GetFileName(filePath));
            }
        }

        private IEnumerator LoadAndPlayAudio(string path)
        {
            string url = "file:///" + path.Replace("\\", "/");
            AudioType type = GetAudioType(path);
            
            using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(url, type))
            {
                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.Success)
                {
                    AudioClip clip = DownloadHandlerAudioClip.GetContent(www);
                    if (clip != null)
                    {
                        audioSource.clip = clip;
                        audioSource.spatialBlend = 0f;
                        audioSource.priority = 0;
                        audioSource.volume = ModFunc.GI().bgmVolume;
                        audioSource.loop = false; // Tắt loop của Unity để tự xử lý qua Update
                        
                        // Dừng nhạc nền gốc nếu nó đang phát
                        if (Sound.isPlayingSoundBG(0))
                        {
                            Sound.sTopSoundBG(); 
                        }

                        audioSource.Play();
                    }
                }
                else
                {
                    SafeAddInfo("Lỗi tải nhạc: " + www.error);
                }
            }
        }

        private AudioType GetAudioType(string path)
        {
            string ext = Path.GetExtension(path).ToLower();
            switch (ext)
            {
                case ".mp3": return AudioType.MPEG;
                case ".ogg": return AudioType.OGGVORBIS;
                case ".wav": return AudioType.WAV;
                case ".aiff": return AudioType.AIFF;
                default: return AudioType.UNKNOWN;
            }
        }
        
        /// <summary>
        /// Helper method để hiển thị thông báo an toàn (kiểm tra null và try-catch)
        /// </summary>
        private void SafeAddInfo(string message)
        {
            // Chỉ hiển thị thông báo nếu game đã load xong
            if (GameCanvas.isLoadRes && GameScr.info1 != null)
            {
                try
                {
                    GameScr.info1.addInfo(message, 0);
                }
                catch
                {
                    Debug.Log("[BgmManager] " + message);
                }
            }
            else
            {
                Debug.Log("[BgmManager] " + message);
            }
        }
    }
}
