type CafeStory = {
  storyText: string;
};

const storyContent = document.querySelector<HTMLElement>('#story-content');
const storyError = document.querySelector<HTMLElement>('#story-error');

function renderStory(story: CafeStory): void {
  if (!storyContent) return;
  storyContent.replaceChildren(
    ...story.storyText.split('\n\n').map((paragraph) => {
      const element = document.createElement('p');
      element.textContent = paragraph;
      return element;
    })
  );
}

async function loadStory(): Promise<void> {
  try {
    const response = await fetch('/api/cafe-story/active');
    if (!response.ok) throw new Error(`Story request failed: ${response.status}`);
    renderStory(await response.json() as CafeStory);
  } catch {
    if (storyContent) storyContent.hidden = true;
    if (storyError) storyError.hidden = false;
  }
}

void loadStory();
