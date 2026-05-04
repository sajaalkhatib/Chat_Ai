
    // سجل المحادثة
    let conversationHistory = [];

    // إرسال رسالة
    async function sendMessage() {
        const userInput = document.getElementById('userInput');
    const message = userInput.value.trim();

    if (!message) return;

    // إضافة رسالة المستخدم للواجهة
    addMessageToUI(message, 'user');
    userInput.value = '';

    // إظهار مؤشر الكتابة
    showTypingIndicator();

    try {
            // إرسال الطلب للـ Controller
            const response = await fetch('/Home/SendMessage', {
        method: 'POST',
    headers: {
        'Content-Type': 'application/json',
                },
    body: JSON.stringify({message: message })
            });

    const data = await response.json();

    // إخفاء مؤشر الكتابة
    hideTypingIndicator();

    // إضافة رد البوت للواجهة
    addMessageToUI(data.reply, 'bot');

    // حفظ المحادثة
    conversationHistory.push({role: 'user', content: message });
    conversationHistory.push({role: 'bot', content: data.reply });

        } catch (error) {
        hideTypingIndicator();
    addMessageToUI('Sorry, there was an error. Please try again.', 'bot');
    console.error('Error:', error);
        }
    }

    // إضافة رسالة للواجهة
    function addMessageToUI(text, sender) {
        const chatMessages = document.getElementById('chatMessages');
    const messageDiv = document.createElement('div');
    messageDiv.className = `message ${sender}`;

    const time = new Date().toLocaleTimeString([], {hour: '2-digit', minute: '2-digit' });

    if (sender === 'bot') {
        messageDiv.innerHTML = `
                <div class="bot-icon"><i class="bi bi-robot"></i></div>
                <div class="message-content">
                    ${escapeHtml(text)}
                    <div class="message-time">${time}</div>
                </div>
            `;
        } else {
        messageDiv.innerHTML = `
                <div class="message-content">
                    ${escapeHtml(text)}
                    <div class="message-time text-end">${time}</div>
                </div>
            `;
        }

    chatMessages.appendChild(messageDiv);
    scrollToBottom();
    }

    // إظهار مؤشر الكتابة
    function showTypingIndicator() {
        const chatMessages = document.getElementById('chatMessages');
    const typingDiv = document.createElement('div');
    typingDiv.className = 'message bot';
    typingDiv.id = 'typingIndicator';
    typingDiv.innerHTML = `
    <div class="bot-icon"><i class="bi bi-robot"></i></div>
    <div class="typing-indicator">
        <span></span>
        <span></span>
        <span></span>
    </div>
    `;
    chatMessages.appendChild(typingDiv);
    scrollToBottom();
    }

    // إخفاء مؤشر الكتابة
    function hideTypingIndicator() {
        const indicator = document.getElementById('typingIndicator');
    if (indicator) {
        indicator.remove();
        }
    }

    // مسح الشات
    function clearChat() {
        const chatMessages = document.getElementById('chatMessages');
    chatMessages.innerHTML = `
    <div class="message bot">
        <div class="bot-icon"><i class="bi bi-robot"></i></div>
        <div class="message-content">
            Chat cleared! How can I help you? 😊
            <div class="message-time">Just now</div>
        </div>
    </div>
    `;
    conversationHistory = [];
    }

    // التمرير للأسفل
    function scrollToBottom() {
        const chatMessages = document.getElementById('chatMessages');
    chatMessages.scrollTop = chatMessages.scrollHeight;
    }

    // معالج Enter
    function handleKeyPress(event) {
        if (event.key === 'Enter') {
        sendMessage();
        }
    }

    // حماية من XSS
    function escapeHtml(text) {
        const div = document.createElement('div');
    div.textContent = text;
    return div.innerHTML;
    }
