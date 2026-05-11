# Breathe Widget V2: Gentle Focus Awareness

## Context

The current application already provides the first version of the product: a transparent, always-on-top, click-through WPF breathing widget with adaptive ambient coloring and tray controls. The codebase also has an initial `WorkState` model and `WorkStateMapper` that can make the breathing profile stronger for focused states, but the application does not yet observe foreground windows, keyboard activity, mouse movement, or automatically show gentle text prompts.

Version 2 should make the widget respond to signs that the user has stayed inside one problem space for a long time. The product tone must stay gentle: it should not accuse the user of being distracted or force an interruption. It should quietly create a moment to return to breath and body awareness.

## Product Direction

Use a lightweight mixed signal model:

- Foreground window dwell time is the primary signal.
- Keyboard and mouse activity refine the strength of the signal.
- Window switching and inactivity reduce the signal.
- Text prompts appear only after sustained deep focus, and only briefly.

This avoids a heavy "AI state detection" system in V2 while still making the reminder feel more context-aware than a fixed timer.

## Architecture

Add three small units rather than putting the behavior directly in `MainWindow`:

1. `ActivityMonitor`
   - Samples the current foreground window at a fixed interval.
   - Tracks window identity, title, process name, dwell duration, last input time, keyboard input count, mouse movement amount, and window switch count.
   - Produces an immutable activity snapshot.

2. `WorkStateEvaluator`
   - Converts the latest activity snapshot into `WorkState.Light`, `WorkState.Focused`, or `WorkState.DeepFocus`.
   - Keeps thresholds centralized and testable.
   - Does not know about WPF visuals.

3. `GentlePromptPolicy`
   - Decides whether a short text prompt should be shown.
   - Suppresses repeated prompts in the same window for a cooldown period.
   - Selects from a small prompt text pool.

`MainWindow` remains responsible for rendering. It subscribes to evaluated state changes, updates the existing `BreathSampler` through `WorkStateMapper`, and fades prompt text in or out when the policy requests it.

## Detection Rules

Initial thresholds should be conservative and easy to tune:

- `Light`
  - Same foreground window dwell is less than 20 minutes, or
  - The user switches windows regularly, or
  - The user has been inactive long enough that the session should be considered broken.

- `Focused`
  - Same foreground window dwell is 20 to 45 minutes, and
  - There is continued keyboard or mouse activity, and
  - Window switching is low.

- `DeepFocus`
  - Same foreground window dwell exceeds 45 minutes, and
  - Keyboard input is sustained or repetitive, and
  - Window switching is very low, and
  - Mouse movement is low, narrow, or repetitive enough to suggest the user may be stuck in one problem space.

These states are intentionally framed as "possible prolonged focus", not as a judgment about the user's mental state.

## Reminder Behavior

`Light`:

- Keep the existing transparent breathing visual.
- Show no text.
- Use the default or subtle breathing profile according to the current user-selected mode.

`Focused`:

- Make the breathing visual slightly more noticeable.
- Use a slower cycle, around 7 seconds.
- Show no text.

`DeepFocus`:

- Make the breathing visual slower and a little more visible.
- Use a cycle around 9 seconds.
- If `DeepFocus` persists for 2 to 3 minutes, fade in one gentle text prompt.
- Keep the prompt visible for 8 to 12 seconds, then fade it out.
- Do not show another prompt for the same foreground window until at least 15 to 20 minutes have passed.

The prompt should be atmospheric, not modal. The overlay must stay click-through and should not steal focus.

## Prompt Copy

Use a small rotating prompt pool:

- "你已经在这里停留很久了。可以先回到一次呼吸。"
- "先不急着解决它，觉察一下身体现在的状态。"
- "让视线离开屏幕几秒，看看呼吸有没有变浅。"
- "你可以暂停一下，不是放弃，只是回来看看自己。"
- "慢慢吸气，慢慢呼气。先把自己找回来。"

The wording should stay soft, non-judgmental, and concise.

## Error Handling And Privacy

- If foreground window inspection fails, fall back to time-based light breathing without prompts.
- If low-level keyboard or mouse hooks are unavailable, keep the app running and evaluate from foreground window dwell time only.
- Do not persist window titles, process names, keyboard counts, or prompt history to disk in V2.
- Do not record actual key contents. Only aggregate counts and movement totals are needed.

## Testing

Add focused unit tests for:

- Foreground window dwell accumulation and reset on window switch.
- `WorkStateEvaluator` thresholds for `Light`, `Focused`, and `DeepFocus`.
- Prompt cooldown and deep-focus persistence behavior.
- No prompt before the deep-focus persistence threshold.
- No repeated prompt inside the cooldown period.

Keep Win32 API calls behind interfaces so evaluator and prompt policy tests do not need real desktop input.

## Out Of Scope For V2

- AI state detection.
- Browser tab content inspection.
- VS Code-specific state integration.
- Audio reminders or text-to-speech prompts.
- Persisted analytics or history.
- Blocking or modal reminders.

## Open Implementation Notes

- The existing `WorkStateMapper` can remain the visual mapping point.
- `MainWindow` should get a second timer for activity evaluation rather than mixing it with the ambient color sampler.
- The prompt text can be added as a WPF `TextBlock` in the transparent canvas with opacity animation.
- Tray controls should eventually include pause and sensitivity settings, but those can be deferred unless implementation reveals that testing the feature without them is awkward.
