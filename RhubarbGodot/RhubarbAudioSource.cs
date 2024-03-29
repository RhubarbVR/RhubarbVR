﻿using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

using Godot;

using NAudio.Wave;

using RhuEngine;
using RhuEngine.Components;
using RhuEngine.Linker;

namespace RhubarbVR
{
	public sealed class RhubarbAudioSource : IDisposable
	{
		public AudioStreamGenerator Audio;
		private AudioStreamGeneratorPlayback _audioPlayBack;
		private AudioSourceBase _audioPlayer;
		private IWaveProvider _waveProvider;

		public RhubarbAudioSource() {
			Audio = new AudioStreamGenerator {
				BufferLength = 0.2f,

			};
			RAudio.UpateAudioSystems += Update;
		}

		public void SetUpAudio(AudioSourceBase audioSourceBase) {
			if (audioSourceBase == null) {
				throw new ArgumentNullException(nameof(audioSourceBase));
			}

			_audioPlayer = audioSourceBase;
			Link();
		}

		public void Instansiate(AudioStreamPlayback audioStreamPlayback) {
			_audioPlayBack = (AudioStreamGeneratorPlayback)audioStreamPlayback;
		}

		private void Link() {
			_audioPlayer.AudioStream.LoadChange += AudioStream_LoadChange;
			AudioStream_LoadChange(_audioPlayer.AudioStream.Asset);
		}

		private void AudioStream_LoadChange(IWaveProvider obj) {
			if (obj == null) {
				return;
			}

			if (RAudio.Inst.WaveFormat.Channels == obj.WaveFormat.Channels && RAudio.Inst.WaveFormat.Encoding == obj.WaveFormat.Encoding && RAudio.Inst.WaveFormat.BitsPerSample == obj.WaveFormat.BitsPerSample) {
				Audio.MixRate = obj.WaveFormat.SampleRate;
				_waveProvider = obj;
				return;
			}
			try {
				// Create new resampler
				Audio.MixRate = obj.WaveFormat.SampleRate;
				_waveProvider = new AudioConverter(RAudio.Inst.WaveFormat, obj, true);

			}
			catch (Exception ex) {
				// Log error and return if resampler creation fails
				GD.PrintErr($"Error creating MediaFoundationResampler: {ex.Message}");
				return;
			}
		}

		private byte[] _audioBuffer = Array.Empty<byte>();

		private unsafe void ReadAudio() {
			if (_audioPlayBack is null) {
				return;
			}
			if (_waveProvider is null) {
				_audioPlayBack.PushBuffer(new Vector2[_audioPlayBack.GetFramesAvailable()]);
				return;
			}
			var audioFrames = _audioPlayBack.GetFramesAvailable();
			if (audioFrames <= 0) {
				return;
			}
			var bytesNeeded = audioFrames * sizeof(Vector2);
			if (bytesNeeded > _audioBuffer.Length) {
				Array.Resize(ref _audioBuffer, bytesNeeded);
			}
			try {
				var readAmount = _waveProvider.Read(_audioBuffer, 0, bytesNeeded) / sizeof(Vector2);
				var audioBuffer = new Vector2[readAmount];
				fixed (byte* byteData = _audioBuffer) {
					var castedPointer = (Vector2*)byteData;
					for (var i = 0; i < readAmount; i++) {
						audioBuffer[i] = castedPointer[i];
					}
				}
				_audioPlayBack.PushBuffer(audioBuffer);
			}
			catch (Exception ex) {
				// Log error and return if audio reading fails
				GD.PrintErr($"Error reading audio: {ex.Message}");
				return;
			}
		}

		public void Dispose() {
			RAudio.UpateAudioSystems -= Update;
			Audio?.Free();
			Audio = null;
			_waveProvider = null;
		}

		private void Update() {
			ReadAudio();
		}
	}
}