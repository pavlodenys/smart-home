<script>
  let faqs = [
    { question: "How do I add a new device?", answer: "To add a new device, navigate to the Devices page and click on the 'Add Device' button. Follow the on-screen instructions to complete the setup process." },
    { question: "How can I contact support?", answer: "If you need assistance or have any questions, you can contact our support team at support@example.com or call our helpline at 123-456-7890." },
    { question: "How do I reset my password?", answer: "To reset your password, click on the 'Forgot Password' link on the login page. Follow the instructions sent to your registered email to set a new password." }
  ];

  let tutorials = [
    { title: "Getting Started Guide", link: "https://example.com/getting-started-guide" },
    { title: "Device Setup Tutorial", link: "https://example.com/device-setup-tutorial" },
    { title: "Advanced Automation Techniques", link: "https://example.com/advanced-automation-techniques" }
  ];
</script>

<style>
  .faq-item {
    margin-bottom: 1rem;
  }

  .faq-question {
    font-weight: bold;
    margin-bottom: 0.5rem;
  }

  .faq-answer {
    margin: 0;
  }

  .tutorial-item {
    margin-bottom: 1rem;
  }
</style>

<div>
  <h2>Help</h2>

  <h3>Frequently Asked Questions</h3>
  {#each faqs as faq}
    <div class="faq-item">
      <p class="faq-question">{faq.question}</p>
      <p class="faq-answer">{faq.answer}</p>
    </div>
  {/each}

  <h3>Tutorials</h3>
  {#each tutorials as tutorial}
    <div class="tutorial-item">
      <a href={tutorial.link}>{tutorial.title}</a>
    </div>
  {/each}
</div>
