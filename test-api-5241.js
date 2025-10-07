const axios = require('axios');

const BASE_URL = 'http://localhost:5241';
process.env.NODE_TLS_REJECT_UNAUTHORIZED = '0';

let token = '';

const formatTime = () => {
    const now = new Date();
    return now.toLocaleTimeString('sr-RS', { 
        hour: '2-digit', 
        minute: '2-digit', 
        second: '2-digit' 
    });
};

async function login() {
    try {
        console.log(`\n🔐 Pokušavam login...`);
        const response = await axios.post(`${BASE_URL}/api/auth/login`, {
            username: 'admin',
            password: 'admin123'
        });
        
        console.log(`[${formatTime()}] ✅ Login uspešan!`);
        
        // Proveri response struktura
        console.log('Response data:', JSON.stringify(response.data, null, 2));
        
        // Pokušaj različite načine da uzmeš token
        token = response.data.token || response.data.Token || response.data.access_token || response.data.accessToken;
        
        if (token) {
            console.log(`Token: ${token.substring(0, 50)}...`);
            return true;
        } else {
            console.log('❌ Token nije pronađen u odgovoru');
            return false;
        }
        
    } catch (error) {
        console.log(`[${formatTime()}] ❌ Greška pri login-u:`);
        console.log(`Status: ${error.response?.status || 'Nema odgovora'}`);
        console.log(`Poruka: ${error.response?.data?.message || error.message}`);
        return false;
    }
}

async function testGetSingleEmployee() {
    try {
        console.log(`\n👤 Testiram GET zaposleni/1...`);
        const response = await axios.get(`${BASE_URL}/api/zaposleni/1`, {
            headers: { 'Authorization': `Bearer ${token}` }
        });
        
        console.log(`[${formatTime()}] ✅ Uspešno dohvaćen zaposleni!`);
        console.log(`${response.data.ime} ${response.data.prezime}`);
        console.log(`Email: ${response.data.email}`);
        return true;
    } catch (error) {
        console.log(`[${formatTime()}] ❌ Greška pri dohvatanju zaposlenog:`);
        console.log(`Status: ${error.response?.status}`);
        console.log(`Timeout: ${error.code === 'ECONNABORTED' ? 'DA' : 'NE'}`);
        console.log(`Poruka: ${error.response?.data?.message || error.message}`);
        return false;
    }
}

async function testMyData() {
    try {
        console.log(`\n🔍 Testiram moji-podaci endpoint...`);
        const response = await axios.get(`${BASE_URL}/api/zaposleni/moji-podaci`, {
            headers: { 'Authorization': `Bearer ${token}` }
        });
        
        console.log(`[${formatTime()}] ✅ Moji podaci uspešno dohvaćeni!`);
        console.log(`${response.data.ime} ${response.data.prezime}`);
        console.log(`Email: ${response.data.email}`);
        return true;
    } catch (error) {
        console.log(`[${formatTime()}] ❌ Greška pri dohvatanju mojih podataka:`);
        console.log(`Status: ${error.response?.status}`);
        console.log(`Timeout: ${error.code === 'ECONNABORTED' ? 'DA' : 'NE'}`);
        console.log(`Poruka: ${error.response?.data?.message || error.message}`);
        return false;
    }
}

async function runTests() {
    console.log('🧙 GLAVNI TEST - DA LI JE DTO FIX REŠIO PROBLEM!\n');
    
    const loginOK = await login();
    if (!loginOK) {
        console.log('\n❌ Bez uspešnog login-a ne mogu da testiram ostale endpoint-e.');
        return;
    }
    
    console.log('\n🎯 OVO SU ENDPOINT-I KOJI SU RANIJE IMALI TIMEOUT:');
    
    // GLAVNI TESTOVI - endpoint-i koji su ranije imali circular reference problem
    const test1 = await testGetSingleEmployee();
    const test2 = await testMyData();
    
    console.log(`\n🎉 REZULTAT:`);
    console.log(`✅ GET /api/zaposleni/1: ${test1 ? 'RADI' : 'NE RADI'}`);
    console.log(`✅ GET /api/zaposleni/moji-podaci: ${test2 ? 'RADI' : 'NE RADI'}`);
    
    if (test1 && test2) {
        console.log(`\n🎊 USPEH! DTO fix je rešio circular reference problem!`);
    } else {
        console.log(`\n❌ Problem još uvek postoji. Treba dodatno debugging.`);
    }
}

runTests().catch(console.error);