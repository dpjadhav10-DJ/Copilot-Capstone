const storyContent = document.querySelector('#story-content');
const storyError = document.querySelector('#story-error');

function renderStory(story) {
  if (!storyContent) return;
  storyContent.replaceChildren(...story.storyText.split('\n\n').map((paragraph) => {
    const element = document.createElement('p');
    element.textContent = paragraph;
    return element;
  }));
}

async function loadStory() {
  try {
    const response = await fetch('/api/cafe-story/active');
    if (!response.ok) throw new Error(`Story request failed: ${response.status}`);
    renderStory(await response.json());
  } catch {
    if (storyContent) storyContent.hidden = true;
    if (storyError) storyError.hidden = false;
  }
}

void loadStory();
