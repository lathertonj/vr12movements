﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Scene6AhhChords : MonoBehaviour
{
	private ChuckSubInstance myChuck;
	private ChuckEventListener myAhhListener;
	public float firstSunLookAmount = 5f;
	public float secondSunLookAmount = 10f;
	private float startSecondSunLookAmount = 0f;

    // Use this for initialization
    void Start()
    {
		myChuck = GetComponent<ChuckSubInstance>();
		myChuck.RunCode( @"
			//-----------------------------------------------------------------------------
			// name: LiSa-Sndbuf2.ck
			// desc: Live sampling utilities for ChucK
			//
			// author: Jack Atherton
			//
			// Combining Dan Trueman's various helper scripts for Lisa
			// https://github.com/ccrma/music220a/blob/master/chuck-examples/special/LiSa-SndBuf.ck
			// http://chuck.cs.princeton.edu/doc/examples/special/LiSa-munger2.ck
			//-----------------------------------------------------------------------------

			// PARAMS
			class AhhSynth extends Chubgraph
			{
				LiSa lisa => outlet;
				
				// spawn rate: how often a new grain is spawned (ms)
				25 =>  float grainSpawnRateMS;
				0 =>  float grainSpawnRateVariationMS;
				0.0 =>  float grainSpawnRateVariationRateMS;
				
				// position: where in the file is a grain (0 to 1)
				0.61 =>  float grainPosition;
				0.2 =>  float grainPositionRandomness;
				
				// grain length: how long is a grain (ms)
				300 =>  float grainLengthMS;
				10 =>  float grainLengthRandomnessMS;
				
				// grain rate: how quickly is the grain scanning through the file
				1.004 =>  float grainRate; // 1.002 == in-tune Ab
				0.015 =>  float grainRateRandomness;
				
				// ramp up/down: how quickly we ramp up / down
				50 =>  float rampUpMS;
				200 =>  float rampDownMS;
				
				// gain: how loud is everything overall
				1 =>  float gainMultiplier;
				
				float myFreq;
				fun float freq( float f )
				{
					f => myFreq;
					60 => Std.mtof => float baseFreq;
					// 1.002 == in tune for 56 for aah4.wav
					// 1.004 == in tune for 60 for aah5.wav
					myFreq / baseFreq * 1.004 => grainRate;
					
					return myFreq;
				}
				
				fun float freq()
				{
					return myFreq;
				}
				
				fun float gain( float g )
				{
					g => lisa.gain;
					return g;
				}
				
				fun float gain()
				{
					return lisa.gain();
				}
				
				
				
				SndBuf buf; 
				me.dir() + ""aah5.wav"" => buf.read;
				buf.length() => lisa.duration;
				// copy samples in
				for( int i; i < buf.samples(); i++ )
				{
					lisa.valueAt( buf.valueAt( i ), i::samp );
				}
				
				
				buf.length() => dur bufferlen;
				
				// LiSa params
				100 => lisa.maxVoices;
				0.1 => lisa.gain;
				true => lisa.loop;
				false => lisa.record;
				
				
				// modulate
				SinOsc freqmod => blackhole;
				0.1 => freqmod.freq;
				
				
				
				0.1 => float maxGain;
				
				fun void SetGain()
				{
					while( true )
					{
						maxGain * gainMultiplier => lisa.gain;
						1::ms => now;
					}
				}
				spork ~ SetGain();
				
				
				fun void SpawnGrains()
				{
					// create grains
					while( true )
					{
						// grain length
						( grainLengthMS + Math.random2f( -grainLengthRandomnessMS / 2, grainLengthRandomnessMS / 2 ) )
						* 1::ms => dur grainLength;
						
						// grain rate
						grainRate + Math.random2f( -grainRateRandomness / 2, grainRateRandomness / 2 ) => float grainRate;
						
						// grain position
						( grainPosition + Math.random2f( -grainPositionRandomness / 2, grainPositionRandomness / 2 ) )
						* bufferlen => dur playPos;
						
						// grain: grainlen, rampup, rampdown, rate, playPos
						spork ~ PlayGrain( grainLength, rampUpMS::ms, rampDownMS::ms, grainRate, playPos);
						
						// advance time (time per grain)
						// PARAM: GRAIN SPAWN RATE
						grainSpawnRateMS::ms  + freqmod.last() * grainSpawnRateVariationMS::ms => now;
						grainSpawnRateVariationRateMS => freqmod.freq;
					}
				}
				spork ~ SpawnGrains();
				
				// sporkee
				fun void PlayGrain( dur grainlen, dur rampup, dur rampdown, float rate, dur playPos )
				{
					lisa.getVoice() => int newvoice;
					
					if(newvoice > -1)
					{
						lisa.rate( newvoice, rate );
						lisa.playPos( newvoice, playPos );
						lisa.rampUp( newvoice, rampup );
						( grainlen - ( rampup + rampdown ) ) => now;
						lisa.rampDown( newvoice, rampdown) ;
						rampdown => now;
					}
				}


			}

			LPF lpf => NRev rev => dac;
			7000 => lpf.freq;
			rev.mix(0.1);

			57 => int A3;
			59 => int B3;
			61 => int Cs4;
			64 => int E4;
			66 => int Fs4;
			68 => int Gs4;
			71 => int B4;
			73 => int Cs5;
			75 => int Ds5;
			78 => int Fs5;
			80 => int Gs5;
			83 => int B5;
			E4 - 12 => int E3;
			E4 + 12 => int E5;
			Gs4 - 12 => int Gs3;

			// beginning of scene notes
			[[Cs4 - 24, Gs4 - 12, E5 - 12, Cs4 - 12],
			[Gs3 - 12, Cs4 - 12, Fs5 - 12, Cs4 - 12]] @=> int notes[][];

			// the actual notes
			[ 
			[A3, E4, Cs5, Gs5],
			[E3, B4, Ds5, Gs5],
			[Cs4 - 12, Gs4, E5, B5],
			[Gs3, Cs4, B4, Fs5]
			] @=> int notes2[][];

			[ 
			[A3, E4, Cs5, B5],
			[E3, B4, Ds5+12, Gs5],
			[Cs4 - 12, Gs4, E5+12, B5],
			[Gs3, Cs4, B4+12, Fs5+12]
			] @=> int notes3[][];

			AhhSynth ahhs[ notes[0].size() ];
			for( int i; i < ahhs.size(); i++ )
			{
				0.8 / ahhs.size() => ahhs[i].gain;
				ahhs[i] => lpf;
			}

			global float scene6SwellIntensity;
			global Event scene6SwellStart;
			fun void PopulateSwellIntensity()
			{
				while( true )
				{
					1::ms => now;
					lpf.gain() => scene6SwellIntensity;
				}
			}
			spork ~ PopulateSwellIntensity();

			fun void DoSwell( dur swellUp, dur sustain, dur swellDown, float max, float end )
			{
				scene6SwellStart.broadcast();
				lpf.gain() => float start;

				now => time startTime;
				while( now - startTime < swellUp )
				{
					( now - startTime ) / swellUp => float elapsed;
					start + ( max - start ) * elapsed => lpf.gain;
					1::ms => now;
				}	

				1 => lpf.gain;
				sustain => now;

				now => startTime;
				while( now - startTime < swellDown )
				{
					( now - startTime ) / swellDown => float elapsed;
					max + ( end - max ) * elapsed => lpf.gain;
					1::ms => now;
				}
				end => lpf.gain;
			}

			0 => global int halfwayThroughScene6Change;

			global Event scene6TimesWhenWeMightDoSceneChange;
			global Event ahhChordChange;
			global Event ahhChordFadeOut;
			global Event scene6StopMakingSound;
			// wait at start of scene
			0 => lpf.gain;
			8::second => now;

			fun void MakeSound()
			{
				while( true )
				{
					if( halfwayThroughScene6Change == 1 )
					{
						notes2 @=> notes;
						// increment it to 2 so now we know we've heard it! hacky bool --> int
						1 +=> halfwayThroughScene6Change;
					}

					if( halfwayThroughScene6Change == 3 )
					{
						notes3 @=> notes;
						// increment it to 4 so we know we've heard it
						1 +=> halfwayThroughScene6Change;
					}


					for( int i; i < notes.size(); i++ )
					{
						for( int j; j < notes[i].size(); j++ )
						{
							// TODO: maybe start the scene with some stuff down the octave (-24).... then switch it to this up the octave stuff (-12)!!
							notes[i][j] - 12 => Std.mtof => ahhs[j].freq;
						}

						// 2::second => now;
						if( halfwayThroughScene6Change > 1 ) 
						{
							// only tell rocks to change colors after halfway through scene change
							ahhChordChange.broadcast();
							2::second => dur upTime;
							1::second => dur sustainTime;
							3.5::second => dur downTime;
							1::second => dur waitTime;
							spork ~ DoSwell( upTime, sustainTime, downTime, 1, 0.0 );
							upTime + sustainTime => now;
							downTime => now;
							ahhChordFadeOut.broadcast();

							// wait
							waitTime - 0.5::second => now;
							
							// check: do a scene change on the second to last one so that the last one has no gravity drop
							if( i == notes.size() - 2 )
							{
								scene6TimesWhenWeMightDoSceneChange.broadcast();
							}

							// rest of wait
							0.5::second => now;
							if( halfwayThroughScene6Change > 3 ) 
							{
								// wait extra at end
								i * 0.5	::second => now;
							}
						}
						else
						{
							DoSwell( 3.5::second, 0::second, 4.5::second, 0.5, 0.01 );
							// wait
							3.5::second => now;
							
							// check
							if( i == notes.size() - 1 )
							{
								scene6TimesWhenWeMightDoSceneChange.broadcast();
							}
								
							// finish waiting
							0.5::second => now;
						}
					}
				}
			}
			spork ~ MakeSound() @=> Shred soundMaker;
			scene6StopMakingSound => now;
			soundMaker.exit();
			// reverb tail
			10::second => now;
		" );

		ChuckEventListener mySecondHalfAdvancer = gameObject.AddComponent<ChuckEventListener>();
		mySecondHalfAdvancer.ListenForEvent( myChuck, "scene6TimesWhenWeMightDoSceneChange", CheckIfWeShouldDoSceneChange );
		ChuckEventListener mySwellCounter = gameObject.AddComponent<ChuckEventListener>();
		mySwellCounter.ListenForEvent( myChuck, "ahhChordFadeOut", CountSwells );
		myAhhListener = gameObject.AddComponent<ChuckEventListener>();
    }

	bool haveSwitchedToSecondHalf = false;
	bool haveSwitchedToEnding = false;
	int numSecondHalfSwells = 0;
	int numEndSwells = 0;

	int secondHalfSwitchPoint = 11;
	void CountSwells()
	{
		numSecondHalfSwells++;
		// remember starting point
		if( numSecondHalfSwells <= secondHalfSwitchPoint )
		{
			startSecondSunLookAmount = Scene6DetectSunLook.sunLookAmount;
		}

		if( haveSwitchedToEnding )
		{
			numEndSwells++;
			// 4 and 5 cus we count the last one of the last cycle as #1
			if( numEndSwells == 4 )
			{
				// fade visuals in 3 seconds (DoEnd has a 12 second fade; overall time 15)
				GetComponent<Scene6DeepenSkyColors>().Invoke( "DoEnd", 3f );
			}
			else if( numEndSwells == 5 )
			{
				// turn off audio
				myChuck.BroadcastEvent( "scene6StopMakingSound" );

				// in 5 seconds, switch to next scene
				Invoke( "SwitchToNextScene", time: 2.5f );
			}
		}
	}

	void SwitchToNextScene()
	{
		SceneManager.LoadScene( "7_ResolvedStrong" );
	}

	void Update()
	{
		if( !haveSwitchedToSecondHalf )
		{
			// swirl up 
			Scene6SwirlingDust.SetDustIntensity( Scene6DetectSunLook.sunLookAmount.PowMapClamp( 0, firstSunLookAmount, 0, 0.5f, 5 ) );
		}

		if( !haveSwitchedToEnding && haveSwitchedToSecondHalf && numSecondHalfSwells >= secondHalfSwitchPoint )
		{
			// swirl up
			Scene6SwirlingDust.SetDustIntensity( ( Scene6DetectSunLook.sunLookAmount - startSecondSunLookAmount )
				.PowMapClamp( 0, secondSunLookAmount, 0, 1, 5 ) );
		}		
	}

	void TurnOffDust()
	{
		Scene6SwirlingDust.TurnOffDustVisualsButLeaveSwirl();
		myAhhListener.StopListening();
	}

	void CheckIfWeShouldDoSceneChange()
	{
		if( !haveSwitchedToSecondHalf && Scene6DetectSunLook.sunLookAmount > firstSunLookAmount ) //  && Scene6DetectSunLook.currentlyLookingAtSun )
		{
			// signal that the switch should happen at the next musically relevant place
			myChuck.SetInt( "halfwayThroughScene6Change", 1 );
			haveSwitchedToSecondHalf = true;
			// turn off the dust on the next chord change
			myAhhListener.ListenForEvent( myChuck, "ahhChordChange", TurnOffDust );
		}

		if( !haveSwitchedToEnding && haveSwitchedToSecondHalf && numSecondHalfSwells >= 11 )
		{
			Scene6DetectSunLook.startCountingLongContinuousLooks = true;
		}

		if( !haveSwitchedToEnding && haveSwitchedToSecondHalf 
			&& ( Scene6DetectSunLook.sunLookAmount - startSecondSunLookAmount > secondSunLookAmount ||
				Scene6DetectSunLook.numLongContinuousLooks > 3 ) 
			&& numSecondHalfSwells >= 15 )
		{
			// switch to ending
			ApplyWindToSeedlings2.DoEnding();
			myChuck.SetInt( "halfwayThroughScene6Change", 3 );
			haveSwitchedToEnding = true;

			// turn on swirl forever
			Scene6SwirlingDust.SetDustIntensity( 1 );
		}
	}


}
